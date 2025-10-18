using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.ViewModels
{
    public class TradingViewModel
    {
        public List<Instrument> AvailableInstruments { get; set; } = new();
        public Dictionary<int, decimal> CurrentPrices { get; set; } = new();
        public List<Trade> ActiveTrades { get; set; } = new();
        public List<Trade> RecentTrades { get; set; } = new();
        public decimal UserBalance { get; set; }
        public TradingStats Stats { get; set; } = new();
        
        // For trade form
        public int SelectedInstrumentId { get; set; }
        
        [Required(ErrorMessage = "Please select a trade type")]
        public TradeType TradeType { get; set; } // ✅ МЕНЯЕМ Direction на TradeType
        
        [Required(ErrorMessage = "Please enter an amount")]
        [Range(1, 1000, ErrorMessage = "Amount must be between $1 and $1000")]
        public decimal Amount { get; set; } = 10;
        
        [Required(ErrorMessage = "Please select a duration")]
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int Duration { get; set; } = 5;
    }

    public class TradingStats
    {
        public int TotalTrades { get; set; }
        public int WonTrades { get; set; }
        public int LostTrades { get; set; }
        public int ActiveTrades { get; set; }
        public decimal WinRate => TotalTrades > 0 ? (decimal)WonTrades / TotalTrades * 100 : 0;
        public decimal TotalInvested { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal ProfitLoss => TotalPayout - TotalInvested;
        public decimal AverageProfit => WonTrades + LostTrades > 0 ? ProfitLoss / (WonTrades + LostTrades) : 0;
        
        // Today's stats
        public int TodayTrades { get; set; }
        public decimal TodayProfitLoss { get; set; }
        public decimal TodayWinRate => TodayTrades > 0 ? (decimal)WonTrades / TodayTrades * 100 : 0;
        
        // Best/worst trade
        public decimal BestTrade { get; set; }
        public decimal WorstTrade { get; set; }
        public decimal LargestWin { get; set; }
        public decimal LargestLoss { get; set; }
        
        // Streaks
        public int CurrentWinStreak { get; set; }
        public int CurrentLossStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public int LongestLossStreak { get; set; }
    }

    public class QuickTradeViewModel
    {
        [Required]
        public int InstrumentId { get; set; }
        
        [Required]
        public TradeType TradeType { get; set; } // ✅ МЕНЯЕМ
        
        [Required]
        [Range(1, 1000)]
        public decimal Amount { get; set; }
        
        [Required]
        [Range(1, 1440)]
        public int Duration { get; set; }
        
        public decimal CurrentPrice { get; set; }
        public decimal PotentialPayout => Amount * 1.8m;
        public string InstrumentSymbol { get; set; } = string.Empty;
    }

    public class TradeResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TradeId { get; set; }
        public Trade? Trade { get; set; }
        public decimal NewBalance { get; set; }
        public decimal Payout { get; set; }
        public bool IsWin { get; set; }
    }

    public class ActiveTradeViewModel
    {
        public int Id { get; set; }
        public string InstrumentSymbol { get; set; } = string.Empty;
        public TradeType TradeType { get; set; } // ✅ МЕНЯЕМ
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; } // ✅ МЕНЯЕМ OpenPrice на EntryPrice
        public decimal CurrentPrice { get; set; }
        public DateTime CreatedAt { get; set; } // ✅ МЕНЯЕМ OpenTime на CreatedAt
        public DateTime ExpiryTime { get; set; } // ✅ ДОБАВЛЯЕМ вместо CloseTime
        public TimeSpan TimeRemaining => ExpiryTime - DateTime.UtcNow;
        public double ProgressPercent 
        { 
            get 
            {
                var total = (ExpiryTime - CreatedAt).TotalMinutes;
                var elapsed = (DateTime.UtcNow - CreatedAt).TotalMinutes;
                return Math.Min(100, Math.Max(0, (elapsed / total) * 100));
            }
        }
        public bool IsExpired => DateTime.UtcNow >= ExpiryTime;
        public decimal? UnrealizedPnl => CalculateUnrealizedPnl();
        
        private decimal? CalculateUnrealizedPnl()
        {
            var isWon = (TradeType == TradeType.Buy && CurrentPrice > EntryPrice) ||
                        (TradeType == TradeType.Sell && CurrentPrice < EntryPrice);
            return isWon ? Amount * 0.8m : -Amount;
        }
    }

    public class TradeHistoryViewModel
    {
        public List<Trade> Trades { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TradeStatus? StatusFilter { get; set; }
        public int? InstrumentIdFilter { get; set; }
        public string SortBy { get; set; } = "CreatedAt"; // ✅ МЕНЯЕМ OpenTime на CreatedAt
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class InstrumentTradingViewModel
    {
        public Instrument Instrument { get; set; } = new();
        public List<Candle> PriceHistory { get; set; } = new();
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal TodayHigh { get; set; }
        public decimal TodayLow { get; set; }
        public decimal Volume { get; set; }
        public List<TimeFrameOption> AvailableTimeFrames { get; set; } = new()
        {
            new TimeFrameOption { Value = "1", Label = "1 Minute" },
            new TimeFrameOption { Value = "5", Label = "5 Minutes" },
            new TimeFrameOption { Value = "15", Label = "15 Minutes" },
            new TimeFrameOption { Value = "60", Label = "1 Hour" },
            new TimeFrameOption { Value = "240", Label = "4 Hours" },
            new TimeFrameOption { Value = "1440", Label = "1 Day" }
        };
        public string SelectedTimeFrame { get; set; } = "5";
    }

    public class TimeFrameOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MarketOverviewViewModel
    {
        public List<InstrumentSummary> Instruments { get; set; } = new();
        public MarketStats MarketStats { get; set; } = new();
        public List<NewsItem> News { get; set; } = new();
    }

    public class InstrumentSummary
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal TodayHigh { get; set; }
        public decimal TodayLow { get; set; }
        public decimal Volume { get; set; }
        public bool IsHot => PriceChangePercent > 1.0m;
        public bool IsCold => PriceChangePercent < -1.0m;
    }

    public class MarketStats
    {
        public int TotalInstruments { get; set; }
        public int ActiveInstruments { get; set; }
        public int UpInstruments { get; set; }
        public int DownInstruments { get; set; }
        public int UnchangedInstruments { get; set; }
        public decimal MarketSentiment => TotalInstruments > 0 ? (decimal)UpInstruments / TotalInstruments * 100 : 0;
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class TradingSessionViewModel
    {
        public DateTime SessionStart { get; set; } = DateTime.UtcNow;
        public List<Trade> SessionTrades { get; set; } = new();
        
        // ✅ ПЕРЕПИСЫВАЕМ ЛОГИКУ ДЛЯ НОВОЙ МОДЕЛИ
        public decimal SessionProfitLoss => SessionTrades
            .Where(t => t.Profit.HasValue)
            .Sum(t => t.Profit.Value);
            
        public int SessionWins => SessionTrades
            .Count(t => t.Status == TradeStatus.Completed && t.Profit > 0);
            
        public int SessionLosses => SessionTrades
            .Count(t => t.Status == TradeStatus.Completed && t.Profit <= 0);
            
        public decimal SessionWinRate => SessionTrades.Count > 0 ? 
            (decimal)SessionWins / SessionTrades.Count * 100 : 0;
            
        public TimeSpan SessionDuration => DateTime.UtcNow - SessionStart;
    }

    // ✅ ДОБАВЛЯЕМ ВСПОМОГАТЕЛЬНЫЙ КЛАСС ДЛЯ РАСЧЕТА СТАТИСТИК
    public static class TradingStatsCalculator
    {
        public static TradingStats CalculateStats(List<Trade> trades)
        {
            var completedTrades = trades.Where(t => t.Status == TradeStatus.Completed).ToList();
            var activeTrades = trades.Where(t => t.Status == TradeStatus.Active).ToList();
            
            return new TradingStats
            {
                TotalTrades = trades.Count,
                ActiveTrades = activeTrades.Count,
                WonTrades = completedTrades.Count(t => t.Profit > 0),
                LostTrades = completedTrades.Count(t => t.Profit <= 0),
                
                TotalInvested = completedTrades.Sum(t => t.Amount),
                TotalPayout = completedTrades.Where(t => t.Profit.HasValue).Sum(t => t.Profit.Value),
                
                TodayTrades = trades.Count(t => t.CreatedAt.Date == DateTime.Today),
                TodayProfitLoss = completedTrades
                    .Where(t => t.CreatedAt.Date == DateTime.Today && t.Profit.HasValue)
                    .Sum(t => t.Profit.Value),
                    
                BestTrade = completedTrades.Where(t => t.Profit.HasValue).Max(t => t.Profit.Value),
                WorstTrade = completedTrades.Where(t => t.Profit.HasValue).Min(t => t.Profit.Value)
            };
        }
    }
}