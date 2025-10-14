using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;

namespace UspeshnyiTrader.Controllers
{
    public class InstrumentsController : Controller
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public InstrumentsController(IInstrumentRepository instrumentRepository)
        {
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            return View(instruments);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            
            var result = instruments.Select(i => new
            {
                id = i.Id,
                symbol = i.Symbol,
                name = i.Name,
                currentPrice = i.CurrentPrice,
                lastUpdate = i.LastPriceUpdate
            });
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrice(int instrumentId)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
            return Json(new { price = instrument?.CurrentPrice ?? 0 });
        }

        [HttpGet]
        public async Task<IActionResult> GetPrices()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            var prices = instruments.ToDictionary(i => i.Id, i => i.CurrentPrice);
            return Json(prices);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrices()
        {
            // Простая имитация обновления цен
            var instruments = await _instrumentRepository.GetActiveAsync();
            var random = new Random();
            
            foreach (var instrument in instruments)
            {
                var changePercent = (random.NextDouble() - 0.5) * 0.1; // ±5%
                instrument.CurrentPrice = Math.Round(instrument.CurrentPrice * (1 + (decimal)changePercent), 4);
                instrument.LastPriceUpdate = DateTime.UtcNow;
            }
            
            await _instrumentRepository.SaveAllAsync();
            
            return Json(new { success = true, message = "Prices updated" });
        }

        [HttpPost]
        public async Task<IActionResult> SimulatePriceChange()
        {
            // Тот же метод что и UpdatePrices
            return await UpdatePrices();
        }

        [HttpPost]
        public async Task<IActionResult> InitializeInstruments()
        {
            // Проверяем есть ли инструменты, если нет - создаем демо
            var existingInstruments = await _instrumentRepository.GetActiveAsync();
            if (!existingInstruments.Any())
            {
                var demoInstruments = new[]
                {
                    new Models.Entities.Instrument 
                    { 
                        Symbol = "EURUSD", 
                        Name = "Euro vs US Dollar",
                        CurrentPrice = 1.0850m,
                        IsActive = true
                    },
                    new Models.Entities.Instrument 
                    { 
                        Symbol = "GBPUSD", 
                        Name = "British Pound vs US Dollar",
                        CurrentPrice = 1.2650m,
                        IsActive = true
                    },
                    new Models.Entities.Instrument 
                    { 
                        Symbol = "USDJPY", 
                        Name = "US Dollar vs Japanese Yen", 
                        CurrentPrice = 148.50m,
                        IsActive = true
                    }
                };
                
                foreach (var instrument in demoInstruments)
                {
                    await _instrumentRepository.AddAsync(instrument);
                }
            }
            
            return Json(new { success = true, message = "Instruments initialized" });
        }
    }
}