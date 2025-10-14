using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;

namespace UspeshnyiTrader.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public HomeController(IInstrumentRepository instrumentRepository)
        {
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var instruments = await _instrumentRepository.GetActiveAsync();
                ViewBag.Instruments = instruments;
                ViewBag.Message = $"БД работает! Инструментов: {instruments.Count()}";
            }
            catch(Exception ex)
            {
                // Если БД не доступна, используем временные данные
                ViewBag.Instruments = new List<object>
                {
                    new { Symbol = "EURUSD", CurrentPrice = 1.0850m },
                    new { Symbol = "GBPUSD", CurrentPrice = 1.2650m },
                    new { Symbol = "USDJPY", CurrentPrice = 148.50m }
                };
                ViewBag.Message = $"БД не работает! {ex.Message}";
            }
            
            return View();
        }
    }
}