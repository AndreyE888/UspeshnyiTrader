using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class Instrument
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; } = string.Empty; // "EURUSD", "BTCUSD"
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Euro vs US Dollar"
        
        public bool IsActive { get; set; } = true;
        
        public decimal CurrentPrice { get; set; }
        
        public DateTime LastPriceUpdate { get; set; }

        // Navigation properties
        public List<Candle> Candles { get; set; } = new();
        public List<Trade> Trades { get; set; } = new();
    }
}