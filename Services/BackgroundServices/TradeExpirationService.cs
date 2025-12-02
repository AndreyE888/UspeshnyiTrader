using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UspeshnyiTrader.Services.BackgroundServices
{
    public class TradeExpirationService : BackgroundService
    {
        private readonly ILogger<TradeExpirationService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10); // Check every 10 seconds

        public TradeExpirationService(ILogger<TradeExpirationService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trade Expiration Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var tradingService = scope.ServiceProvider.GetRequiredService<ITradingService>();
                        await tradingService.ProcessExpiredTradesAsync();
                    }

                    _logger.LogInformation("Processed expired trades");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired trades");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Trade Expiration Service stopped");
        }
    }
}