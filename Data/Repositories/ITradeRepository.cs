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
        Task<int> GetCountAsync();
        Task<int> GetActiveCountAsync();
        Task<int> GetTodayCountAsync();
    
        // Новые методы для админки
        Task<int> GetSuccessfulTradesCountAsync();
        Task<decimal> GetTotalVolumeAsync();
        Task<decimal> GetPlatformProfitAsync();
        Task<List<Trade>> GetUserTradesAsync(int userId);
        
        // Методы для работы с результатами сделок
        Task<List<Trade>> GetWinningTradesAsync(int userId);
        Task<List<Trade>> GetLosingTradesAsync(int userId);
        Task<List<Trade>> GetDrawTradesAsync(int userId);
        Task<Dictionary<TradeResult, int>> GetTradeResultStatsAsync(int userId);
    }
}