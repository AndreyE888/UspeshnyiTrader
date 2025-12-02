using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Services
{
    public interface IPriceService
    {
        Task<decimal> GetCurrentPriceAsync(int instrumentId);
        Task UpdateAllPricesAsync();
        Task<Dictionary<int, decimal>> GetAllPricesAsync();
        Task SimulatePriceChangeAsync();
        Task InitializeInstrumentsAsync();
    }
}