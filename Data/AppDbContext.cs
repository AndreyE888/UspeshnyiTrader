using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Candle> Candles { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserBalance> UserBalances { get; set; }
        public DbSet<DistributedCache> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
              
            modelBuilder.Entity<DistributedCache>(entity =>
            {
                entity.HasKey(e => e.Id);
        
                entity.HasIndex(e => e.ExpiresAtTime)
                    .HasDatabaseName("IX_Sessions_ExpiresAtTime");
            
                entity.Property(e => e.Id)
                    .HasMaxLength(449);
            
                entity.Property(e => e.Value)
                    .IsRequired();
            });

            // Configure Candle entity
            modelBuilder.Entity<Candle>()
                .HasIndex(c => new { c.InstrumentId, c.Time, c.Interval })
                .IsUnique();

            // Configure Instrument entity
            modelBuilder.Entity<Instrument>()
                .HasIndex(i => i.Symbol)
                .IsUnique();

            // Configure Trade entity
            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.InstrumentId);

            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.Status);
            
            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.Result);

            modelBuilder.Entity<Trade>()
                .Property(t => t.Result)
                .HasConversion<string>() // Сохраняем как строку в БД (например: "Win", "Loss", "Draw", "Pending")
                .HasMaxLength(20) // Максимальная длина строки
                .HasDefaultValue(TradeResult.Pending); // Значение по умолчанию
            
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure UserBalance entity
            modelBuilder.Entity<UserBalance>()
                .HasIndex(ub => ub.UserId);

            // Configure relationships
            modelBuilder.Entity<Candle>()
                .HasOne(c => c.Instrument)
                .WithMany(i => i.Candles)
                .HasForeignKey(c => c.InstrumentId);

            modelBuilder.Entity<Trade>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trades)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Trade>()
                .HasOne(t => t.Instrument)
                .WithMany(i => i.Trades)
                .HasForeignKey(t => t.InstrumentId);

            modelBuilder.Entity<UserBalance>()
                .HasOne(b => b.User)              // У каждой записи баланса один пользователь
                .WithMany(u => u.BalanceHistories) // У пользователя много записей баланса
                .HasForeignKey(b => b.UserId)      // Внешний ключ
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}