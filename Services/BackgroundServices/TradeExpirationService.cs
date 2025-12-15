using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UspeshnyiTrader.Services.BackgroundServices
{
    public class TradeExpirationService : BackgroundService
    {
        private readonly ILogger<TradeExpirationService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1); // Check every 1 seconds

        public TradeExpirationService(ILogger<TradeExpirationService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🔥🔥🔥🔥🔥 TRADE EXPIRATION SERVICE ЗАПУЩЕН! 🔥🔥🔥🔥🔥");
            Console.WriteLine($"Время запуска: {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"Будет проверять каждые {_checkInterval.TotalSeconds} секунд");
            _logger.LogInformation("Trade Expiration Service started");


            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"\n🔄 {DateTime.Now:HH:mm:ss} - НАЧАЛО проверки истекших сделок...");

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        Console.WriteLine($"1. Создан scope, получаю TradingService...");
                        var tradingService = scope.ServiceProvider.GetRequiredService<ITradingService>();
                        Console.WriteLine($"✅ TradingService получен");

                        Console.WriteLine($"2. Вызываю ProcessExpiredTradesAsync...");
                        await tradingService.ProcessExpiredTradesAsync();
                        Console.WriteLine($"✅ ProcessExpiredTradesAsync завершен");
                    }

                    Console.WriteLine($"✅ Проверка завершена успешно");
                    _logger.LogInformation("Processed expired trades");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥💥💥 ОШИБКА в сервисе экспирации:");
                    Console.WriteLine($"Сообщение: {ex.Message}");
                    Console.WriteLine($"Тип: {ex.GetType().Name}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                    }

                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    _logger.LogError(ex, "Error processing expired trades");
                }

                Console.WriteLine($"⏳ Жду {_checkInterval.TotalSeconds} секунд до следующей проверки...");
                await Task.Delay(_checkInterval, stoppingToken);
            }

            Console.WriteLine("🛑 Trade Expiration Service остановлен");
            _logger.LogInformation("Trade Expiration Service stopped");
        }
    }
}