using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class Candle
    {
        public int Id { get; set; }
        
        [Required]
        public int InstrumentId { get; set; }
        
        [Required]
        public DateTime Time { get; set; }
        
        [Required]
        public TimeSpan Interval { get; set; }
        
        [Required]
        public decimal Open { get; set; }
        
        [Required]
        public decimal High { get; set; }
        
        [Required]
        public decimal Low { get; set; }
        
        [Required]
        public decimal Close { get; set; }
        
        [Required]
        public long Volume { get; set; }

        // Navigation property
        public Instrument Instrument { get; set; } = null!;
    }
}