using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public enum TradeStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }

    public enum TradeType
    {
        Buy,
        Sell
    }

    // НОВЫЙ ENUM ДЛЯ РЕЗУЛЬТАТА
    public enum TradeResult
    {
        Pending, // Сделка еще не закрыта
        Win, // Выигрыш
        Loss, // Проигрыш  
        Draw // Ничья (возврат средств)
    }

    public class Trade
    {
        public int Id { get; set; }

        [Required] public int UserId { get; set; }
        public User User { get; set; }

        [Required] public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }

        [Required] public TradeType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal EntryPrice { get; set; }

        public decimal? ExitPrice { get; set; }

        public decimal? Profit { get; set; }

        [Required] public TradeStatus Status { get; set; } = TradeStatus.Active;

        // ЗАМЕНЯЕМ bool? IsWin НА TradeResult
        public TradeResult Result { get; set; } = TradeResult.Pending;

        // Дополнительные свойства для удобства (опционально)
        public bool IsWin => Result == TradeResult.Win;
        public bool IsLoss => Result == TradeResult.Loss;
        public bool IsDraw => Result == TradeResult.Draw;
        public bool HasResult => Result != TradeResult.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        [Required] public DateTime OpenTime { get; set; } = DateTime.UtcNow;
        public DateTime? CloseTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Investment { get; set; }
        public decimal? Payout { get; set; }

        public TimeSpan TimeRemaining => ExpirationTime - DateTime.UtcNow;
        public bool IsExpired => DateTime.UtcNow >= ExpirationTime;

        public void SetExpiration(int durationMinutes)
        {
            Duration = TimeSpan.FromMinutes(durationMinutes);
            ExpirationTime = OpenTime.Add(Duration);
            Investment = Amount;
        }

    }
}