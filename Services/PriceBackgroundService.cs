using UspeshnyiTrader.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UspeshnyiTrader.Services
{
    public class PriceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceBackgroundService> _logger;
        private readonly Random _random = new Random();

        public PriceBackgroundService(IServiceProvider serviceProvider, ILogger<PriceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("💰 Price Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var instrumentRepository = scope.ServiceProvider.GetRequiredService<IInstrumentRepository>();
                    
                    await UpdatePrices(instrumentRepository);
                    _logger.LogInformation("✅ Prices updated at {Time}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error updating prices");
                }

                // Ждем 5 секунд перед следующим обновлением
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        private async Task UpdatePrices(IInstrumentRepository instrumentRepository)
        {
            var instruments = await instrumentRepository.GetActiveAsync();
            
            foreach (var instrument in instruments)
            {
                // Реалистичное изменение цены (±0.5%)
                var changePercent = (_random.NextDouble() - 0.5) * 0.002;
                var newPrice = instrument.CurrentPrice * (1 + (decimal)changePercent);
                
                // Округляем в зависимости от инструмента
                if (instrument.Symbol.Contains("JPY"))
                    newPrice = Math.Round(newPrice, 4); // JPY - 4 знака
                else if (instrument.Symbol.Contains("XAU"))
                    newPrice = Math.Round(newPrice, 4); // Gold - 2 знака
                else
                    newPrice = Math.Round(newPrice, 4); // Forex - 4 знака
                
                // Обновляем только если цена изменилась
                if (newPrice != instrument.CurrentPrice)
                {
                    instrument.CurrentPrice = newPrice;
                    instrument.LastPriceUpdate = DateTime.UtcNow;
                }
            }
            
            await instrumentRepository.SaveAllAsync();
        }
    }
}