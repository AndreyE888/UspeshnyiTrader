using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Models.ViewModels
{
    public class PlaceTradeRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public TradeType Direction { get; set; }
        public decimal Amount { get; set; }
        public int DurationMinutes { get; set; } = 1;
    }

    public class TradeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TradeData? Trade { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime ExpirationTime { get; set; }
        public TimeSpan TimeRemaining { get; set; }
    }

    public class TradeData
    {
        public int Id { get; set; }
        public string InstrumentSymbol { get; set; } = string.Empty;
        public TradeType Direction { get; set; }
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Profit { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpirationTime;
    }

    public class ActiveTradesResponse
    {
        public bool Success { get; set; }
        public List<TradeData> ActiveTrades { get; set; } = new();
        public int Count => ActiveTrades.Count;
    }

    public class CloseTradeRequest
    {
        public int TradeId { get; set; }
        public decimal? ClosePrice { get; set; }
    }

    public class TradeResultResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsWin { get; set; }
        public decimal Payout { get; set; }
        public decimal Profit { get; set; }
        public decimal NewBalance { get; set; }
        public TradeData? Trade { get; set; }
    }

    public class MarketDataResponse
    {
        public bool Success { get; set; }
        public Dictionary<string, decimal> Prices { get; set; } = new();
        public Dictionary<string, PriceChange> Changes { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class PriceChange
    {
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public bool IsPositive => Change > 0;
    }
}