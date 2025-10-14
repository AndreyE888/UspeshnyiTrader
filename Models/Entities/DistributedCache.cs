using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UspeshnyiTrader.Models.Entities
{
    [Table("Sessions")]
    public class DistributedCache
    {
        [Key]
        [StringLength(449)]
        public string Id { get; set; }

        [Required]
        public byte[] Value { get; set; }

        public DateTimeOffset? ExpiresAtTime { get; set; }

        public long? SlidingExpirationInSeconds { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}