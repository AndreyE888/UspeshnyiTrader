using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Services;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Controllers
{
    public class TradingController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly ISessionService _sessionService;

        public TradingController(
            IUserRepository userRepository, 
            IInstrumentRepository instrumentRepository,
            ITradeRepository tradeRepository,
            ISessionService sessionService)
        {
            _userRepository = userRepository;
            _instrumentRepository = instrumentRepository;
            _tradeRepository = tradeRepository;
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                _sessionService.Logout();
                return RedirectToAction("Login", "Account");
            }

            var instruments = await _instrumentRepository.GetActiveAsync();
            
            ViewBag.UserName = user.Username;
            ViewBag.UserBalance = user.Balance;
            ViewBag.UserId = user.Id;
            ViewBag.Instruments = instruments;

            return View();
        }
        
        public async Task<IActionResult> Trade(string symbol)
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                _sessionService.Logout();
                return RedirectToAction("Login", "Account");
            }

            var instrument = await _instrumentRepository.GetBySymbolAsync(symbol);
            if (instrument == null)
            {
                TempData["Error"] = "Инструмент не найден";
                return RedirectToAction("Index");
            }

            ViewBag.Symbol = symbol;
            ViewBag.InstrumentName = instrument.Name;
            ViewBag.CurrentPrice = instrument.CurrentPrice;
            ViewBag.UserName = user.Username;
            ViewBag.UserBalance = user.Balance;
            ViewBag.UserId = user.Id;

            return View();
        }

        public async Task<IActionResult> History()
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var trades = await _tradeRepository.GetUserTradesAsync(userId.Value);
            
            ViewBag.Trades = trades;
            ViewBag.UserId = userId.Value;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string symbol, string direction, decimal amount, decimal price)
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Требуется авторизация" });

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return Json(new { success = false, message = "Пользователь не найден" });

            if (user.Balance < amount)
                return Json(new { success = false, message = "Недостаточно средств" });

            var instrument = await _instrumentRepository.GetBySymbolAsync(symbol);
            if (instrument == null)
                return Json(new { success = false, message = "Инструмент не найден" });

            try
            {
                // ✅ ИСПРАВЛЯЕМ: Используем правильные поля из модели Trade
                var trade = new Trade
                {
                    UserId = userId.Value,
                    InstrumentId = instrument.Id,
                    Type = direction.ToLower() == "up" ? TradeType.Buy : TradeType.Sell, // ✅ МЕНЯЕМ Direction на Type
                    Amount = amount,
                    EntryPrice = price, // ✅ МЕНЯЕМ OpenPrice на EntryPrice
                    CreatedAt = DateTime.UtcNow, // ✅ МЕНЯЕМ OpenTime на CreatedAt
                    Status = TradeStatus.Active
                };

                user.Balance -= amount;
                await _userRepository.UpdateAsync(user);

                await _tradeRepository.AddAsync(trade);

                return Json(new { 
                    success = true, 
                    message = "Ордер размещен успешно!",
                    tradeId = trade.Id,
                    newBalance = user.Balance
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CloseTrade(int tradeId, decimal closePrice)
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Требуется авторизация" });

            var trade = await _tradeRepository.GetByIdAsync(tradeId);
            if (trade == null || trade.UserId != userId.Value)
                return Json(new { success = false, message = "Сделка не найдена" });

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return Json(new { success = false, message = "Пользователь не найден" });

            try
            {
                // ✅ ИСПРАВЛЯЕМ: Рассчитываем результат на основе правильных полей
                decimal result = 0;
                bool isWin = false;

                if ((trade.Type == TradeType.Buy && closePrice > trade.EntryPrice) ||
                    (trade.Type == TradeType.Sell && closePrice < trade.EntryPrice))
                {
                    // Выигрыш
                    result = trade.Amount * 0.8m; // 80% выплата
                    isWin = true;
                }

                // Обновляем баланс
                user.Balance += result;
                await _userRepository.UpdateAsync(user);

                // ✅ ИСПРАВЛЯЕМ: Используем правильные поля для закрытия сделки
                trade.ExitPrice = closePrice; // ✅ МЕНЯЕМ ClosePrice на ExitPrice
                trade.ClosedAt = DateTime.UtcNow; // ✅ МЕНЯЕМ CloseTime на ClosedAt
                trade.Status = TradeStatus.Completed; // ✅ МЕНЯЕМ Closed на Completed
                trade.Profit = result; // ✅ МЕНЯЕМ Payout на Profit

                await _tradeRepository.UpdateAsync(trade);

                return Json(new { 
                    success = true, 
                    message = isWin ? "Сделка закрыта с прибылью!" : "Сделка закрыта",
                    isWin = isWin,
                    payout = result,
                    newBalance = user.Balance
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentPrices()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            var prices = instruments.ToDictionary(i => i.Symbol, i => i.CurrentPrice);
            
            return Json(new { success = true, prices = prices });
        }
    }
}