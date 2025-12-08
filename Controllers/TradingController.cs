using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Services;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;
using UspeshnyiTrader.Data;

namespace UspeshnyiTrader.Controllers
{
    public class TradingController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly ISessionService _sessionService;
        private readonly ITradingService _tradingService;
        private readonly AppDbContext _context; // ДОБАВЬ ЭТУ СТРОЧКУ


        public TradingController(
            IUserRepository userRepository,
            IInstrumentRepository instrumentRepository,
            ITradeRepository tradeRepository,
            ISessionService sessionService,
            ITradingService tradingService,
            AppDbContext context)
        {
            _userRepository = userRepository;
            _instrumentRepository = instrumentRepository;
            _tradeRepository = tradeRepository;
            _sessionService = sessionService;
            _tradingService = tradingService;
            _context = context;
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
            var currentInstrument =
                await _instrumentRepository.GetBySymbolAsync(symbol) ?? instruments.FirstOrDefault();

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
                var trade = await _tradingService.OpenTradeAsync(

                    userId.Value,
                    instrument.Id,
                    request.Direction.ToLower() == "up" ? TradeType.Buy : TradeType.Sell,
                    request.Amount,
                    request.DurationMinutes
                );



                return Json(new
                {
                    success = true,
                    message = "Ордер размещен успешно!",
                    tradeId = trade.Id,
                    newBalance = trade.User.Balance,
                    timeRemaining = trade.TimeRemaining.ToString(@"mm\:ss"),
                    expirationTime = trade.ExpirationTime.ToString("HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                var fullError = ex.ToString();
                Console.WriteLine($"Ошибка: {fullError}");
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

        [HttpGet]
        public async Task<IActionResult> GetAvailableInstruments()
        {
            var instruments = await _instrumentRepository.GetActiveAsync();
            var result = instruments.Select(i => new
            {
                id = i.Id,
                symbol = i.Symbol,
                name = i.Name,
                currentPrice = i.CurrentPrice,
                description = i.Description,
                imageUrl = i.ImageUrl
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetInstrumentPrice(int instrumentId)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
            if (instrument == null)
                return Json(new { price = 0 });

            return Json(new
            {
                price = instrument.CurrentPrice,
                lastUpdate = instrument.LastPriceUpdate
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBalance()
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { balance = 0, username = "Guest" });

            var user = await _userRepository.GetByIdAsync(userId.Value);
            return Json(new
            {
                balance = user?.Balance ?? 0,
                username = user?.Username ?? "Unknown"
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetUserTrades(int limit = 10)
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            var userTrades = await _tradeRepository.GetByUserIdAsync(userId.Value);

            var tradesData = userTrades.Select(t => new
                {
                    id = t.Id,
                    instrumentSymbol = t.Instrument?.Symbol,
                    direction = t.Type.ToString(),
                    amount = t.Amount,
                    openPrice = t.EntryPrice,
                    closePrice = t.ExitPrice,
                    status = t.Status.ToString(),
                    profit = t.Profit,
                    openTime = t.CreatedAt,
                    closeTime = t.ClosedAt,
                    duration = t.Duration.TotalMinutes,
                    payout = t.Profit > 0 ? t.Amount + t.Profit : 0,
                    isWin = t.Profit > 0
                }).OrderByDescending(t => t.openTime)
                .Take(limit)
                .ToList();

            return Json(new { success = true, trades = tradesData });
        }

        [HttpGet]
        public async Task<IActionResult> GetTradeResult(int tradeId)  // ← параметр из query string
        {
            var userId = _sessionService.GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            var trade = await _tradeRepository.GetByIdAsync(tradeId);
            if (trade == null || trade.UserId != userId.Value)
                return Json(new { success = false, message = "Trade not found" });

            if (trade.Status != TradeStatus.Completed)
                return Json(new { success = false, message = "Trade not completed yet" });

            return Json(new
            {
                success = true,
                status = trade.Status.ToString(), // ← ДОБАВЬ ЭТО!
                isCompleted = trade.Status == TradeStatus.Completed,
                isActive = trade.Status == TradeStatus.Active,
                isWin = trade.Profit > 0,
                profit = trade.Profit,
                payout = trade.Profit > 0 ? trade.Amount + trade.Profit : 0,
                exitPrice = trade.ExitPrice,
                entryPrice = trade.EntryPrice,
                amount = trade.Amount,
                closedAt = trade.ClosedAt?.ToString("HH:mm:ss")
            });
        }


        // Модель для запроса на размещение ордера
        public class PlaceOrderRequest
        {
            public string Symbol { get; set; }
            public string Direction { get; set; }
            public decimal Amount { get; set; }
            public decimal Price { get; set; }
            public int DurationMinutes { get; set; } = 1;
        }

    }
}