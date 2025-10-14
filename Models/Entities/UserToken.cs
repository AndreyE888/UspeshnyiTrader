using System.ComponentModel.DataAnnotations;

namespace UspeshnyiTrader.Models.Entities
{
    public class UserToken
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRevoked { get; set; } = false;
        
        // Navigation property
        public User User { get; set; } = null!;
    }
}