using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface ITradeRepository
    {
        Task<Trade?> GetByIdAsync(int id);
        Task<List<Trade>> GetByUserIdAsync(int userId);
        Task<List<Trade>> GetActiveTradesAsync();
        Task<List<Trade>> GetExpiredTradesAsync();
        Task<List<Trade>> GetByInstrumentIdAsync(int instrumentId);
        Task<List<Trade>> GetByStatusAsync(TradeStatus status);
        Task AddAsync(Trade trade);
        Task UpdateAsync(Trade trade);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<Trade>> GetTradesToCloseAsync(DateTime currentTime);
    }
}