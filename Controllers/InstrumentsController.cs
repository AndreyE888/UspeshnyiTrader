using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;

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

        // НОВЫЙ МЕТОД: Страница изменений в прайсе
        public async Task<IActionResult> PriceChanges()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            
            // Создаем историю изменений (в реальном проекте была бы отдельная таблица)
            var priceChanges = instruments.Select(i => new
            {
                Instrument = i.Symbol,
                Name = i.Name,
                CurrentPrice = i.CurrentPrice,
                OldPrice = Math.Round(i.CurrentPrice * 0.995m, 4), // -0.5% для примера
                Change = "+0.5%",
                LastUpdate = i.LastPriceUpdate ?? DateTime.UtcNow
            }).ToList();

            ViewBag.PriceChanges = priceChanges;
            return View();
        }

        // НОВЫЙ МЕТОД: Детальная страница инструмента
        public async Task<IActionResult> Details(int id)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            return View(instrument);
        }

        // НОВЫЙ МЕТОД: API для получения истории цен (для графиков)
        [HttpGet]
        public IActionResult GetPriceHistory(int instrumentId, string period = "1d")
        {
            // Демо-данные для графика (в реальном проекте из БД)
            var demoData = new[]
            {
                new { time = DateTime.UtcNow.AddHours(-24), price = 1.0800m },
                new { time = DateTime.UtcNow.AddHours(-18), price = 1.0820m },
                new { time = DateTime.UtcNow.AddHours(-12), price = 1.0835m },
                new { time = DateTime.UtcNow.AddHours(-6), price = 1.0840m },
                new { time = DateTime.UtcNow, price = 1.0850m }
            };

            return Json(demoData);
        }

        // СУЩЕСТВУЮЩИЕ МЕТОДЫ (оставляем как есть):
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
            var instruments = await _instrumentRepository.GetActiveAsync();
            var random = new Random();
            
            foreach (var instrument in instruments)
            {
                var changePercent = (random.NextDouble() - 0.5) * 0.1;
                instrument.CurrentPrice = Math.Round(instrument.CurrentPrice * (1 + (decimal)changePercent), 4);
                instrument.LastPriceUpdate = DateTime.UtcNow;
            }
            
            await _instrumentRepository.SaveAllAsync();
            
            return Json(new { success = true, message = "Prices updated" });
        }

        [HttpPost]
        public async Task<IActionResult> SimulatePriceChange()
        {
            return await UpdatePrices();
        }

        [HttpPost]
        public async Task<IActionResult> InitializeInstruments()
        {
            var existingInstruments = await _instrumentRepository.GetActiveAsync();
            if (!existingInstruments.Any())
            {
                var demoInstruments = new[]
                {
                    new Instrument 
                    { 
                        Symbol = "EURUSD", 
                        Name = "Euro vs US Dollar",
                        CurrentPrice = 1.0850m,
                        IsActive = true,
                        Description = "Валютная пара Евро/Доллар США"
                    },
                    new Instrument 
                    { 
                        Symbol = "GBPUSD", 
                        Name = "British Pound vs US Dollar",
                        CurrentPrice = 1.2650m,
                        IsActive = true,
                        Description = "Валютная пара Фунт Стерлингов/Доллар США"
                    },
                    new Instrument 
                    { 
                        Symbol = "USDJPY", 
                        Name = "US Dollar vs Japanese Yen", 
                        CurrentPrice = 148.50m,
                        IsActive = true,
                        Description = "Валютная пара Доллар США/Японская Йена"
                    },
                    new Instrument 
                    { 
                        Symbol = "XAUUSD", 
                        Name = "Gold vs US Dollar", 
                        CurrentPrice = 2020.50m,
                        IsActive = true,
                        Description = "Золото к Доллару США"
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