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

        // ОБЪЕДИНЕННЫЙ МЕТОД: и график, и торговля на одной странице
        public async Task<IActionResult> Index(string symbol = "BTCUSD")
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
            var currentInstrument = await _instrumentRepository.GetBySymbolAsync(symbol) ?? instruments.FirstOrDefault();

            // Если нет инструментов, создаем демо-данные
            if (!instruments.Any())
            {
                instruments = GetDemoInstruments();
                currentInstrument = instruments.First();
            }

            // Передаем данные через ViewBag и ViewModel
            ViewBag.UserName = user.Username;
            ViewBag.UserBalance = user.Balance;
            ViewBag.UserId = user.Id;
            ViewBag.Instruments = instruments;
            ViewBag.SelectedSymbol = currentInstrument?.Symbol ?? "BTCUSD";
            ViewBag.CurrentPrice = currentInstrument?.CurrentPrice ?? 50000.00m;

            return View();
        }

        // УДАЛЯЕМ старый метод Trade - теперь все в Index
        // public async Task<IActionResult> Trade(string symbol) - УДАЛИТЬ

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
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Требуется авторизация" });

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return Json(new { success = false, message = "Пользователь не найден" });

            if (user.Balance < request.Amount)
                return Json(new { success = false, message = "Недостаточно средств" });

            var instrument = await _instrumentRepository.GetBySymbolAsync(request.Symbol);
            if (instrument == null)
                return Json(new { success = false, message = "Инструмент не найден" });

            try
            {
                var trade = new Trade
                {
                    UserId = userId.Value,
                    InstrumentId = instrument.Id,
                    Type = request.Direction.ToLower() == "up" ? TradeType.Buy : TradeType.Sell,
                    Amount = request.Amount,
                    EntryPrice = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    Status = TradeStatus.Active
                };

                user.Balance -= request.Amount;
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
                decimal result = 0;
                bool isWin = false;

                if ((trade.Type == TradeType.Buy && closePrice > trade.EntryPrice) ||
                    (trade.Type == TradeType.Sell && closePrice < trade.EntryPrice))
                {
                    result = trade.Amount * 0.8m;
                    isWin = true;
                }

                user.Balance += result;
                await _userRepository.UpdateAsync(user);

                trade.ExitPrice = closePrice;
                trade.ClosedAt = DateTime.UtcNow;
                trade.Status = TradeStatus.Completed;
                trade.Profit = result;

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

        // Демо-инструменты на случай если база пустая
        private List<Instrument> GetDemoInstruments()
        {
            return new List<Instrument>
            {
                new Instrument { Symbol = "BTCUSD", Name = "Bitcoin/USD", CurrentPrice = 50000.00m },
                new Instrument { Symbol = "ETHUSD", Name = "Ethereum/USD", CurrentPrice = 3000.00m },
                new Instrument { Symbol = "EURUSD", Name = "Euro/US Dollar", CurrentPrice = 1.0850m },
                new Instrument { Symbol = "GBPUSD", Name = "British Pound/USD", CurrentPrice = 1.2650m },
                new Instrument { Symbol = "AAPL", Name = "Apple Inc.", CurrentPrice = 180.00m },
                new Instrument { Symbol = "GOOGL", Name = "Alphabet Inc.", CurrentPrice = 2700.00m }
            };
        }
    }

    // Модель для запроса на размещение ордера
    public class PlaceOrderRequest
    {
        public string Symbol { get; set; }
        public string Direction { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}