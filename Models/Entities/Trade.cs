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

    public class Trade
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
        
        [Required]
        public TradeType Type { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal EntryPrice { get; set; }
        
        public decimal? ExitPrice { get; set; }
        
        public decimal? Profit { get; set; }  // ✅ ДОБАВЛЯЕМ
        
        [Required]
        public TradeStatus Status { get; set; } = TradeStatus.Active; // ✅ ДОБАВЛЯЕМ
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        
        [Required]
        public DateTime OpenTime { get; set; } = DateTime.UtcNow;
        public DateTime? CloseTime { get; set; }
        public DateTime ExpirationTime { get; set; } // время экспирации
        public TimeSpan Duration { get; set; } //длительность опциона
        public decimal Investment { get; set; } //сумма инвестиции
        public decimal? Payout { get; set; } //потенциальная выплата
        public bool? IsWin { get; set; } //результат опциона

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