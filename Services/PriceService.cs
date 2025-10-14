using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Services
{
    public class PriceService : IPriceService
    {
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ICandleRepository _candleRepository;
        private readonly Random _random;

        public PriceService(IInstrumentRepository instrumentRepository, ICandleRepository candleRepository)
        {
            _instrumentRepository = instrumentRepository;
            _candleRepository = candleRepository;
            _random = new Random();
        }

        public async Task<decimal> GetCurrentPriceAsync(int instrumentId)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
            return instrument?.CurrentPrice ?? 0;
        }

        public async Task UpdateAllPricesAsync()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            foreach (var instrument in instruments)
            {
                await UpdateInstrumentPriceAsync(instrument);
            }
        }

        public async Task<Dictionary<int, decimal>> GetAllPricesAsync()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            return instruments.ToDictionary(i => i.Id, i => i.CurrentPrice);
        }

        public async Task SimulatePriceChangeAsync()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            foreach (var instrument in instruments)
            {
                // Simulate small price movement (±0.5%)
                var changePercent = (_random.NextDouble() - 0.5) * 0.01;
                var change = (decimal)changePercent * instrument.CurrentPrice;
                var newPrice = instrument.CurrentPrice + change;

                // Ensure price doesn't go below 0.0001
                newPrice = Math.Max(newPrice, 0.0001m);

                await _instrumentRepository.UpdatePriceAsync(instrument.Id, newPrice);

                // Create candle record
                var candle = new Candle
                {
                    InstrumentId = instrument.Id,
                    Time = DateTime.UtcNow,
                    Interval = TimeSpan.FromMinutes(1),
                    Open = instrument.CurrentPrice,
                    High = Math.Max(instrument.CurrentPrice, newPrice),
                    Low = Math.Min(instrument.CurrentPrice, newPrice),
                    Close = newPrice,
                    Volume = _random.Next(100, 10000)
                };

                await _candleRepository.AddAsync(candle);
            }
        }

        public async Task InitializeInstrumentsAsync()
        {
            var defaultInstruments = new[]
            {
                new Instrument { Symbol = "EURUSD", Name = "Euro vs US Dollar", CurrentPrice = 1.0850m, LastPriceUpdate = DateTime.UtcNow },
                new Instrument { Symbol = "GBPUSD", Name = "British Pound vs US Dollar", CurrentPrice = 1.2650m, LastPriceUpdate = DateTime.UtcNow },
                new Instrument { Symbol = "USDJPY", Name = "US Dollar vs Japanese Yen", CurrentPrice = 148.50m, LastPriceUpdate = DateTime.UtcNow },
                new Instrument { Symbol = "BTCUSD", Name = "Bitcoin vs US Dollar", CurrentPrice = 45000.00m, LastPriceUpdate = DateTime.UtcNow },
                new Instrument { Symbol = "ETHUSD", Name = "Ethereum vs US Dollar", CurrentPrice = 2500.00m, LastPriceUpdate = DateTime.UtcNow },
                new Instrument { Symbol = "XAUUSD", Name = "Gold vs US Dollar", CurrentPrice = 1980.00m, LastPriceUpdate = DateTime.UtcNow }
            };

            foreach (var instrument in defaultInstruments)
            {
                if (!await _instrumentRepository.SymbolExistsAsync(instrument.Symbol))
                {
                    await _instrumentRepository.AddAsync(instrument);
                }
            }
        }

        private async Task UpdateInstrumentPriceAsync(Instrument instrument)
        {
            // For demo purposes, simulate price change
            // In real application, this would fetch from external API
            var changePercent = (_random.NextDouble() - 0.5) * 0.02; // ±1%
            var change = (decimal)changePercent * instrument.CurrentPrice;
            var newPrice = instrument.CurrentPrice + change;

            // Ensure reasonable minimum price
            newPrice = Math.Max(newPrice, 0.0001m);

            await _instrumentRepository.UpdatePriceAsync(instrument.Id, newPrice);
        }
    }
}