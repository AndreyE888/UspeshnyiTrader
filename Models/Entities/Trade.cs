using System.ComponentModel.DataAnnotations;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Models.Entities
{
    public class Trade
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int InstrumentId { get; set; }
        
        [Required]
        public TradeDirection Direction { get; set; } // Up/Down
        
        [Required]
        [Range(1, 10000)]
        public decimal Amount { get; set; } // Сумма ставки
        
        [Required]
        public decimal OpenPrice { get; set; } // Цена при открытии
        
        public decimal? ClosePrice { get; set; } // Цена при закрытии
        
        [Required]
        public DateTime OpenTime { get; set; }
        
        [Required]
        public DateTime CloseTime { get; set; } // Время экспирации
        
        [Required]
        public TradeStatus Status { get; set; } // Active/Won/Lost
        
        public decimal? Payout { get; set; } // Выигрыш
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Instrument Instrument { get; set; } = null!;
    }
}