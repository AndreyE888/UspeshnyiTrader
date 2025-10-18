using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Enums;
using UspeshnyiTrader.Models.Entities;

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
            direction = t.Type.ToString(), // ✅ МЕНЯЕМ Direction на Type
            amount = t.Amount,
            openPrice = t.EntryPrice, // ✅ МЕНЯЕМ OpenPrice на EntryPrice
            closePrice = t.ExitPrice, // ✅ МЕНЯЕМ ClosePrice на ExitPrice
            payout = t.Profit, // ✅ МЕНЯЕМ Payout на Profit
            status = t.Status.ToString(),
            openTime = t.CreatedAt, // ✅ МЕНЯЕМ OpenTime на CreatedAt
            closeTime = t.ClosedAt  // ✅ МЕНЯЕМ CloseTime на ClosedAt
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
            
            var result = filteredTrades.Select(t => new
            {
                id = t.Id,
                instrumentSymbol = t.Instrument?.Symbol,
                direction = t.Type.ToString(),
                amount = t.Amount,
                openPrice = t.EntryPrice,
                closePrice = t.ExitPrice,
                payout = t.Profit,
                status = t.Status.ToString(),
                openTime = t.CreatedAt,
                closeTime = t.ClosedAt
            });
            
            return Json(result);
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
            direction = trade.Type.ToString(), // ✅ МЕНЯЕМ
            amount = trade.Amount,
            openPrice = trade.EntryPrice, // ✅ МЕНЯЕМ
            closePrice = trade.ExitPrice, // ✅ МЕНЯЕМ
            payout = trade.Profit, // ✅ МЕНЯЕМ
            status = trade.Status.ToString(),
            openTime = trade.CreatedAt, // ✅ МЕНЯЕМ
            closeTime = trade.ClosedAt, // ✅ МЕНЯЕМ
            duration = trade.ClosedAt.HasValue ? 
                      (trade.ClosedAt.Value - trade.CreatedAt).TotalMinutes : 0 // ✅ ИСПРАВЛЯЕМ
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
        var wonTrades = trades.Count(t => t.Status == TradeStatus.Completed && t.Profit > 0); // ✅ ИСПРАВЛЯЕМ
        var lostTrades = trades.Count(t => t.Status == TradeStatus.Completed && t.Profit <= 0); // ✅ ИСПРАВЛЯЕМ
        var activeTrades = trades.Count(t => t.Status == TradeStatus.Active);
        
        var totalInvested = trades.Where(t => t.Status != TradeStatus.Active).Sum(t => t.Amount);
        var totalPayout = trades.Where(t => t.Profit.HasValue).Sum(t => t.Profit.Value); // ✅ МЕНЯЕМ Payout на Profit
        var profitLoss = totalPayout;

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