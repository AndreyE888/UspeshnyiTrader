using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public HistoryController(ITradeRepository tradeRepository, IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var trades = await _tradeRepository.GetByUserIdAsync(userId.Value);
            return View(trades);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { error = "Not authenticated" });

            var trades = await _tradeRepository.GetByUserIdAsync(userId.Value);
            
            var result = trades.Select(t => new
            {
                id = t.Id,
                instrumentSymbol = t.Instrument?.Symbol,
                direction = t.Direction.ToString(),
                amount = t.Amount,
                openPrice = t.OpenPrice,
                closePrice = t.ClosePrice,
                payout = t.Payout,
                status = t.Status.ToString(),
                openTime = t.OpenTime,
                closeTime = t.CloseTime
            });
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryByStatus(string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { error = "Not authenticated" });

            if (Enum.TryParse<TradeStatus>(status, true, out var tradeStatus))
            {
                var allTrades = await _tradeRepository.GetByUserIdAsync(userId.Value);
                var filteredTrades = allTrades.Where(t => t.Status == tradeStatus);
                
                return Json(filteredTrades);
            }

            return Json(new { error = "Invalid status" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTradeDetails(int id)
        {
            var trade = await _tradeRepository.GetByIdAsync(id);
            if (trade == null)
                return Json(new { error = "Trade not found" });

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || trade.UserId != userId.Value)
                return Json(new { error = "Access denied" });

            var result = new
            {
                id = trade.Id,
                instrument = trade.Instrument?.Symbol,
                direction = trade.Direction.ToString(),
                amount = trade.Amount,
                openPrice = trade.OpenPrice,
                closePrice = trade.ClosePrice,
                payout = trade.Payout,
                status = trade.Status.ToString(),
                openTime = trade.OpenTime,
                closeTime = trade.CloseTime,
                duration = (trade.CloseTime - trade.OpenTime).TotalMinutes
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { error = "Not authenticated" });

            var trades = await _tradeRepository.GetByUserIdAsync(userId.Value);
            
            var totalTrades = trades.Count;
            var wonTrades = trades.Count(t => t.Status == TradeStatus.Won);
            var lostTrades = trades.Count(t => t.Status == TradeStatus.Lost);
            var activeTrades = trades.Count(t => t.Status == TradeStatus.Active);
            
            var totalInvested = trades.Where(t => t.Status != TradeStatus.Active).Sum(t => t.Amount);
            var totalPayout = trades.Where(t => t.Payout.HasValue).Sum(t => t.Payout.Value);
            var profitLoss = totalPayout - totalInvested;

            return Json(new
            {
                totalTrades,
                wonTrades,
                lostTrades,
                activeTrades,
                winRate = totalTrades > 0 ? (double)wonTrades / totalTrades : 0,
                totalInvested,
                totalPayout,
                profitLoss
            });
        }
    }
}