using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Data.Repositories
{
    public class TradeRepository : ITradeRepository
    {
        private readonly AppDbContext _context;

        public TradeRepository(AppDbContext context)
        {
            _context = context;
            // Отключаем отслеживание по умолчанию для ВСЕХ запросов
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<Trade?> GetByIdAsync(int id)
        {
            return await _context.Trades
                .AsNoTracking() // Уже не нужно, но оставляем для ясности
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Trade>> GetByUserIdAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking() // ← ДОБАВЬ!
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            // УПРОЩАЕМ: убираем Include, они вызывают проблемы
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.Status == TradeStatus.Active)
                .ToListAsync(); // ← БЕЗ Include!
        }

        public async Task<List<Trade>> GetExpiredTradesAsync()
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Trades
                .AsNoTracking() // ← ДОБАВЬ!
                .Where(t => t.Status == TradeStatus.Active && t.CloseTime <= currentTime)
                .ToListAsync(); // ← БЕЗ Include!
        }

        public async Task<List<Trade>> GetByInstrumentIdAsync(int instrumentId)
        {
            return await _context.Trades
                .AsNoTracking() // ← ДОБАВЬ!
                .Where(t => t.InstrumentId == instrumentId)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync(); // ← БЕЗ Include!
        }

        public async Task<List<Trade>> GetByStatusAsync(TradeStatus status)
        {
            return await _context.Trades
                .AsNoTracking() // ← ДОБАВЬ!
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync(); // ← БЕЗ Include!
        }

        public async Task<List<Trade>> GetTradesToCloseAsync(DateTime currentTime)
        {
            return await _context.Trades
                .AsNoTracking() // ← ДОБАВЬ!
                .Where(t => t.Status == TradeStatus.Active && t.CloseTime <= currentTime)
                .ToListAsync(); // ← БЕЗ Include!
        }

        public async Task AddAsync(Trade trade)
        {
            try
            {
                // Для добавления включаем отслеживание временно
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                
                await _context.Trades.AddAsync(trade);
                await _context.SaveChangesAsync();
                
                // Сразу отсоединяем сущность
                _context.Entry(trade).State = EntityState.Detached;
            }
            finally
            {
                // Возвращаем NoTracking
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        public async Task UpdateAsync(Trade trade)
        {
            try
            {
                // Включаем отслеживание для обновления
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                
                // Проверяем, не отслеживается ли уже сущность с таким ID
                var existingEntity = _context.ChangeTracker.Entries<Trade>()
                    .FirstOrDefault(e => e.Entity.Id == trade.Id);
                    
                if (existingEntity != null)
                {
                    // Отсоединяем существующую
                    existingEntity.State = EntityState.Detached;
                }
                
                // Обновляем сущность
                _context.Entry(trade).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                // Отсоединяем после сохранения
                _context.Entry(trade).State = EntityState.Detached;
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        public async Task UpdateSafeAsync(Trade trade)
        {
            // АЛЬТЕРНАТИВНЫЙ МЕТОД: обновление через прямой SQL
            var sql = @"
                UPDATE ""Trades"" 
                SET ""Status"" = {0},
                    ""ExitPrice"" = {1},
                    ""Result"" = {2},
                    ""ClosedAt"" = {3},
                    ""Profit"" = {4},
                    ""Payout"" = {5}
                WHERE ""Id"" = {6}";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                (int)trade.Status,
                trade.ExitPrice,
                trade.Result.ToString(),
                trade.ClosedAt,
                trade.Profit,
                trade.Payout,
                trade.Id);
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                
                var trade = await _context.Trades
                    .FirstOrDefaultAsync(t => t.Id == id);
                    
                if (trade != null)
                {
                    _context.Trades.Remove(trade);
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Trades
                .AsNoTracking()
                .AnyAsync(t => t.Id == id);
        }
        
        public async Task<int> GetCountAsync()
        {
            return await _context.Trades
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.Status == TradeStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.CreatedAt.Date == today)
                .CountAsync();
        }

        public async Task<int> GetSuccessfulTradesCountAsync()
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.Status == TradeStatus.Completed && t.Result == TradeResult.Win)
                .CountAsync();
        }

        public async Task<decimal> GetTotalVolumeAsync()
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.Status == TradeStatus.Completed)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetPlatformProfitAsync()
        {
            var totalVolume = await GetTotalVolumeAsync();
            return totalVolume * 0.001m;
        }
        
        public async Task<List<Trade>> GetUserTradesAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        // НОВЫЙ МЕТОД: Получить только ID активных сделок (для ProcessExpiredTradesAsync)
        public async Task<List<int>> GetActiveTradeIdsAsync()
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.Status == TradeStatus.Active)
                .Select(t => t.Id)
                .ToListAsync();
        }
        
        // НОВЫЙ МЕТОД: Проверить истекла ли сделка
        public async Task<bool> IsTradeExpiredAsync(int tradeId)
        {
            var expirationTime = await _context.Trades
                .AsNoTracking()
                .Where(t => t.Id == tradeId && t.Status == TradeStatus.Active)
                .Select(t => t.ExpirationTime)
                .FirstOrDefaultAsync();
                
            return expirationTime != default && expirationTime < DateTime.UtcNow;
        }
        
        public async Task<List<Trade>> GetWinningTradesAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Result == TradeResult.Win)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Trade>> GetLosingTradesAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Result == TradeResult.Loss)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Trade>> GetDrawTradesAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Result == TradeResult.Draw)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<Dictionary<TradeResult, int>> GetTradeResultStatsAsync(int userId)
        {
            return await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Status == TradeStatus.Completed)
                .GroupBy(t => t.Result)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Result, x => x.Count);
        }
    }
}