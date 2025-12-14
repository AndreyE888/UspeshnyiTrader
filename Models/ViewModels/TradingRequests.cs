using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Models.ViewModels
{
    public class PlaceTradeRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public TradeType TradeType { get; set; }
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

// ⚠️ Если используется TradeData класс, тоже нужно проверить:
    public class TradeData
    {
        public int Id { get; set; }
        public string InstrumentSymbol { get; set; } = string.Empty;
        public TradeType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal? ExitPrice { get; set; }
    
        // ⚠️ Тоже заменить если есть:
        public TradeResult Result { get; set; } = TradeResult.Pending;
    
        // Для обратной совместимости:
        public bool IsWin => Result == TradeResult.Win;
    
        public decimal? Profit { get; set; }
        public TradeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
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
        // ⚠️ ИЗМЕНЕНО: Добавляем полную информацию о результате
        public TradeResult Result { get; set; } // Основное поле
    
        // ⚠️ ДОБАВЛЕНО: Для обратной совместимости и удобства
        public bool IsWin => Result == TradeResult.Win;
        public bool IsLoss => Result == TradeResult.Loss;
        public bool IsDraw => Result == TradeResult.Draw;
        
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