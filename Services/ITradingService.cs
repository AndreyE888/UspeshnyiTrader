using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Services
{
    public interface ITradingService
    {
        Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeType tradeType, decimal amount, int durationMinutes);
        Task CloseTradeAsync(int tradeId);
        Task ProcessExpiredTradesAsync();
        Task<decimal> CalculatePotentialProfitAsync(decimal amount, TradeType tradeType, decimal currentPrice, decimal entryPrice);
        Task<bool> CanUserTradeAsync(int userId, decimal amount);
        Task<List<Trade>> GetUserTradesAsync(int userId);
        Task<List<Trade>> GetActiveTradesAsync();
        
        // Дополнительные методы для удобства
        Task<decimal> GetUserBalanceAsync(int userId);
        Task<List<Trade>> GetUserCompletedTradesAsync(int userId);
        Task<Dictionary<string, object>> GetTradingStatsAsync(int userId);
    }
}