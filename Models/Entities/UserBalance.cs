using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class UserBalance
    {
            public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        [StringLength(200)]
        public string? Description { get; set; } // "Trade win", "Deposit", "Withdrawal"
        
        [Required]
        public decimal BalanceAfter { get; set; } // Баланс после операции

        // Navigation property
        public User User { get; set; } = null!;
    }
}