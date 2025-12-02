using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface ICandleRepository
    {
        Task<Candle?> GetByIdAsync(int id);
        Task<List<Candle>> GetByInstrumentIdAsync(int instrumentId);
        Task<List<Candle>> GetLastCandlesAsync(int instrumentId, int count, TimeSpan interval);
        Task<Candle?> GetLastCandleAsync(int instrumentId, TimeSpan interval);
        Task AddAsync(Candle candle);
        Task AddRangeAsync(IEnumerable<Candle> candles);
        Task UpdateAsync(Candle candle);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}