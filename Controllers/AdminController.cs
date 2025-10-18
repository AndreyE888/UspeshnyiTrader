using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Controllers
{
    [Authorize(Roles = "Admin")] // Только для администраторов
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ITradeRepository _tradeRepository;

        public AdminController(
            IUserRepository userRepository,
            IInstrumentRepository instrumentRepository,
            ITradeRepository tradeRepository)
        {
            _userRepository = userRepository;
            _instrumentRepository = instrumentRepository;
            _tradeRepository = tradeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalUsers = await _userRepository.GetCountAsync(),
                TotalInstruments = await _instrumentRepository.GetCountAsync(),
                TotalTrades = await _tradeRepository.GetCountAsync(),
                ActiveTrades = await _tradeRepository.GetActiveCountAsync(),
                TodayTrades = await _tradeRepository.GetTodayCountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userTrades = await _tradeRepository.GetUserTradesAsync(id);
            ViewBag.UserTrades = userTrades;
            
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserBalance(int userId, decimal newBalance)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Balance = newBalance;
                await _userRepository.UpdateAsync(user);
                
                TempData["SuccessMessage"] = "Баланс пользователя успешно обновлен";
            }
            
            return RedirectToAction("UserDetails", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _userRepository.UpdateAsync(user);
                
                TempData["SuccessMessage"] = "Пользователь заблокирован";
            }
            
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                await _userRepository.UpdateAsync(user);
                
                TempData["SuccessMessage"] = "Пользователь разблокирован";
            }
            
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Content()
        {
            // Здесь будет управление контентом (новости, инструменты и т.д.)
            var instruments = await _instrumentRepository.GetAllAsync();
            return View(instruments);
        }

        [HttpPost]
        public async Task<IActionResult> AddInstrument(string symbol, string name, decimal price)
        {
            var instrument = new Instrument
            {
                Symbol = symbol,
                Name = name,
                CurrentPrice = price,
                IsActive = true,
                LastPriceUpdate = DateTime.UtcNow
            };

            await _instrumentRepository.AddAsync(instrument);
            TempData["SuccessMessage"] = "Инструмент успешно добавлен";
            
            return RedirectToAction("Content");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInstrumentPrice(int instrumentId, decimal newPrice)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
            if (instrument != null)
            {
                instrument.CurrentPrice = newPrice;
                instrument.LastPriceUpdate = DateTime.UtcNow;
                await _instrumentRepository.UpdateAsync(instrument);
                
                TempData["SuccessMessage"] = "Цена инструмента обновлена";
            }
            
            return RedirectToAction("Content");
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = new
            {
                // Статистика пользователей
                NewUsersToday = await _userRepository.GetNewUsersTodayCountAsync(),
                TotalDeposits = await _userRepository.GetTotalDepositsAsync(),
                
                // Статистика торгов
                TotalTrades = await _tradeRepository.GetCountAsync(),
                SuccessfulTrades = await _tradeRepository.GetSuccessfulTradesCountAsync(),
                TotalVolume = await _tradeRepository.GetTotalVolumeAsync(),
                
                // Финансовая статистика
                PlatformProfit = await _tradeRepository.GetPlatformProfitAsync()
            };

            ViewBag.Statistics = stats;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTradingData(string period)
        {
            // Данные для графиков (демо)
            var tradingData = new[]
            {
                new { Date = "2024-12-01", Trades = 45, Volume = 12500 },
                new { Date = "2024-12-02", Trades = 52, Volume = 14200 },
                new { Date = "2024-12-03", Trades = 48, Volume = 13800 },
                new { Date = "2024-12-04", Trades = 61, Volume = 16800 },
                new { Date = "2024-12-05", Trades = 55, Volume = 15200 }
            };

            return Json(tradingData);
        }
    }
}