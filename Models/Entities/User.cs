using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public decimal Balance { get; set; } = 1000; // Стартовый баланс
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastLogin { get; set; }

        // Navigation properties
        public List<Trade> Trades { get; set; } = new();
        public UserBalance BalanceHistory { get; set; } = null!;
        
    }
}