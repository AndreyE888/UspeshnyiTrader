using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Services
{
    public interface ITradingService
    {
        Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeDirection direction, decimal amount, int durationMinutes);
        Task CloseTradeAsync(int tradeId);
        Task ProcessExpiredTradesAsync();
        Task<decimal> CalculatePayoutAsync(decimal amount, TradeDirection direction, decimal currentPrice, decimal openPrice);
        Task<bool> CanUserTradeAsync(int userId, decimal amount);
        Task<List<Trade>> GetUserTradesAsync(int userId);
        Task<List<Trade>> GetActiveTradesAsync();
    }
}