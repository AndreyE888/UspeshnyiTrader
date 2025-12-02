using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface IInstrumentRepository
    {
        Task<Instrument?> GetByIdAsync(int id);
        Task<Instrument?> GetBySymbolAsync(string symbol);
        Task<List<Instrument>> GetAllAsync();
        Task<List<Instrument>> GetActiveAsync();
        Task AddAsync(Instrument instrument);
        Task UpdateAsync(Instrument instrument);
        Task UpdatePriceAsync(int instrumentId, decimal newPrice);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> SymbolExistsAsync(string symbol);
        Task SaveAllAsync();
        Task<int> GetCountAsync();
        
    }
}