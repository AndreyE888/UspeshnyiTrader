using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class Instrument
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        public decimal CurrentPrice { get; set; }
        
        public string Description { get; set; }
        
        // НОВОЕ ПОЛЕ: Путь к изображению
        public string ImageUrl { get; set; } = "/images/instruments/default.png";
        
        public bool IsActive { get; set; } = true;
        public DateTime? LastPriceUpdate { get; set; }
        
        // Навигационные свойства
        public ICollection<Candle> Candles { get; set; } = new List<Candle>();
        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }
}