using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.ViewModels
{
    public class HistoryViewModel
    {
        public List<Trade> Trades { get; set; } = new();
        public HistoryFilters Filters { get; set; } = new();
        public HistoryStats Stats { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
        public List<Instrument> AvailableInstruments { get; set; } = new();
    }

    public class HistoryFilters
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Status")]
        public TradeStatus? Status { get; set; }

        [Display(Name = "Instrument")]
        public int? InstrumentId { get; set; }

        [Display(Name = "Direction")]
        public TradeDirection? Direction { get; set; }

        [Display(Name = "Min Amount")]
        [Range(1, 1000, ErrorMessage = "Amount must be between $1 and $1000")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        [Range(1, 1000, ErrorMessage = "Amount must be between $1 and $1000")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "OpenTime";

        [Display(Name = "Sort Order")]
        public SortOrder SortOrder { get; set; } = SortOrder.Descending;

        [Display(Name = "Results Per Page")]
        [Range(10, 100, ErrorMessage = "Results per page must be between 10 and 100")]
        public int PageSize { get; set; } = 20;

        public bool HasFilters => 
            StartDate.HasValue || EndDate.HasValue || Status.HasValue || 
            InstrumentId.HasValue || Direction.HasValue || 
            MinAmount.HasValue || MaxAmount.HasValue;

        public void ClearFilters()
        {
            StartDate = null;
            EndDate = null;
            Status = null;
            InstrumentId = null;
            Direction = null;
            MinAmount = null;
            MaxAmount = null;
        }
    }

    public class HistoryStats
    {
        public int TotalTrades { get; set; }
        public int WonTrades { get; set; }
        public int LostTrades { get; set; }
        public int ActiveTrades { get; set; }
        public int CancelledTrades { get; set; }
        
        public decimal WinRate => TotalTrades > 0 ? (decimal)WonTrades / TotalTrades * 100 : 0;
        
        public decimal TotalInvested { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal TotalProfitLoss => TotalPayout - TotalInvested;
        
        public decimal AverageTradeAmount => TotalTrades > 0 ? TotalInvested / TotalTrades : 0;
        public decimal AverageProfitPerTrade => (WonTrades + LostTrades) > 0 ? TotalProfitLoss / (WonTrades + LostTrades) : 0;
        
        // Time-based stats
        public decimal TodayProfitLoss { get; set; }
        public decimal ThisWeekProfitLoss { get; set; }
        public decimal ThisMonthProfitLoss { get; set; }
        public decimal ThisYearProfitLoss { get; set; }
        
        // Best/worst performing
        public decimal BestTradeProfit { get; set; }
        public decimal WorstTradeLoss { get; set; }
        public decimal LargestWinAmount { get; set; }
        public decimal LargestLossAmount { get; set; }
        
        // Streaks
        public int CurrentWinStreak { get; set; }
        public int CurrentLossStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public int LongestLossStreak { get; set; }
        
        // Instrument performance
        public Dictionary<string, InstrumentPerformance> InstrumentPerformance { get; set; } = new();
        
        // Monthly performance
        public Dictionary<string, decimal> MonthlyPerformance { get; set; } = new();
    }

    public class InstrumentPerformance
    {
        public string Symbol { get; set; } = string.Empty;
        public int TotalTrades { get; set; }
        public int WonTrades { get; set; }
        public int LostTrades { get; set; }
        public decimal WinRate => TotalTrades > 0 ? (decimal)WonTrades / TotalTrades * 100 : 0;
        public decimal TotalProfitLoss { get; set; }
        public decimal AverageProfitPerTrade => TotalTrades > 0 ? TotalProfitLoss / TotalTrades : 0;
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int StartItem => ((CurrentPage - 1) * PageSize) + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
        
        public List<int> PageSizes { get; set; } = new() { 10, 20, 50, 100 };
    }

    public enum SortOrder
    {
        Ascending = 1,
        Descending = 2
    }

    public class TradeExportViewModel
    {
        [Display(Name = "Export Format")]
        public ExportFormat Format { get; set; } = ExportFormat.CSV;

        [Display(Name = "Include Columns")]
        public List<string> IncludedColumns { get; set; } = new()
        {
            "TradeId", "Instrument", "Direction", "Amount", "OpenPrice", 
            "ClosePrice", "Payout", "Status", "OpenTime", "CloseTime", "Duration"
        };

        [Display(Name = "Date Range")]
        public DateRange DateRange { get; set; } = DateRange.AllTime;

        [Display(Name = "Custom Start Date")]
        [DataType(DataType.Date)]
        public DateTime? CustomStartDate { get; set; }

        [Display(Name = "Custom End Date")]
        [DataType(DataType.Date)]
        public DateTime? CustomEndDate { get; set; }

        public List<Trade> Trades { get; set; } = new();
    }

    public enum ExportFormat
    {
        CSV = 1,
        Excel = 2,
        PDF = 3,
        JSON = 4
    }

    public enum DateRange
    {
        [Display(Name = "All Time")]
        AllTime = 1,
        
        [Display(Name = "Today")]
        Today = 2,
        
        [Display(Name = "This Week")]
        ThisWeek = 3,
        
        [Display(Name = "This Month")]
        ThisMonth = 4,
        
        [Display(Name = "Last 30 Days")]
        Last30Days = 5,
        
        [Display(Name = "Last 90 Days")]
        Last90Days = 6,
        
        [Display(Name = "This Year")]
        ThisYear = 7,
        
        [Display(Name = "Custom Range")]
        Custom = 8
    }

    public class PerformanceChartViewModel
    {
        public List<PerformanceDataPoint> EquityCurve { get; set; } = new();
        public List<PerformanceDataPoint> DailyProfitLoss { get; set; } = new();
        public List<PerformanceDataPoint> WinRateOverTime { get; set; } = new();
        public List<PerformanceDataPoint> TradeSizeDistribution { get; set; } = new();
        
        public decimal StartingBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalGrowth => CurrentBalance - StartingBalance;
        public decimal GrowthPercentage => StartingBalance > 0 ? (TotalGrowth / StartingBalance) * 100 : 0;
        
        public decimal MaxDrawdown { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal ProfitFactor { get; set; }
    }

    public class PerformanceDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class TradeAnalysisViewModel
    {
        public Trade Trade { get; set; } = new();
        public Instrument Instrument { get; set; } = new();
        public List<Candle> PriceHistory { get; set; } = new();
        
        public decimal MarketMove => Trade.ClosePrice.HasValue ? 
            ((Trade.ClosePrice.Value - Trade.OpenPrice) / Trade.OpenPrice) * 100 : 0;
        
        public decimal RequiredMove => Trade.Direction == TradeDirection.Up ? 0.01m : -0.01m;
        
        public bool WasCloseCall => Math.Abs(MarketMove - RequiredMove) <= 0.1m;
        
        public TimeSpan TradeDuration => Trade.CloseTime - Trade.OpenTime;
        
        public string Analysis 
        { 
            get 
            {
                if (!Trade.ClosePrice.HasValue) return "Trade is still active";
                
                var won = Trade.Status == TradeStatus.Won;
                var moveDirection = MarketMove >= 0 ? "up" : "down";
                var requiredDirection = Trade.Direction == TradeDirection.Up ? "up" : "down";
                
                return won ? 
                    $"Correctly predicted {requiredDirection} movement. Market moved {moveDirection} by {Math.Abs(MarketMove):F2}%" :
                    $"Incorrect prediction. Expected {requiredDirection} but market moved {moveDirection} by {Math.Abs(MarketMove):F2}%";
            }
        }
        
        public List<SimilarTradesAnalysis> SimilarTrades { get; set; } = new();
    }

    public class SimilarTradesAnalysis
    {
        public string Condition { get; set; } = string.Empty;
        public int TotalTrades { get; set; }
        public int WonTrades { get; set; }
        public decimal WinRate => TotalTrades > 0 ? (decimal)WonTrades / TotalTrades * 100 : 0;
        public decimal AverageProfit { get; set; }
    }
}