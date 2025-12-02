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
        }

        public async Task<Trade?> GetByIdAsync(int id)
        {
            return await _context.Trades
                .Include(t => t.User)
                .Include(t => t.Instrument)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Trade>> GetByUserIdAsync(int userId)
        {
            return await _context.Trades
                .Where(t => t.UserId == userId)
                .Include(t => t.Instrument)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Active)
                .Include(t => t.Instrument)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetExpiredTradesAsync()
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Active && t.CloseTime <= currentTime)
                .Include(t => t.Instrument)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetByInstrumentIdAsync(int instrumentId)
        {
            return await _context.Trades
                .Where(t => t.InstrumentId == instrumentId)
                .Include(t => t.User)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetByStatusAsync(TradeStatus status)
        {
            return await _context.Trades
                .Where(t => t.Status == status)
                .Include(t => t.Instrument)
                .Include(t => t.User)
                .OrderByDescending(t => t.OpenTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetTradesToCloseAsync(DateTime currentTime)
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Active && t.CloseTime <= currentTime)
                .Include(t => t.Instrument)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task AddAsync(Trade trade)
        {
            await _context.Trades.AddAsync(trade);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Trade trade)
        {
            _context.Trades.Update(trade);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var trade = await GetByIdAsync(id);
            if (trade != null)
            {
                _context.Trades.Remove(trade);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Trades.AnyAsync(t => t.Id == id);
        }
        
        public async Task<int> GetCountAsync()
        {
            return await _context.Trades.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Trades
                .Where(t => t.CreatedAt.Date == today)
                .CountAsync();
        }

        public async Task<int> GetSuccessfulTradesCountAsync()
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Completed && t.Profit > 0)
                .CountAsync();
        }

        public async Task<decimal> GetTotalVolumeAsync()
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Completed)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetPlatformProfitAsync()
        {
            // Предположим, что комиссия платформы - 0.1% от объема
            var totalVolume = await GetTotalVolumeAsync();
            return totalVolume * 0.001m; // 0.1% комиссия
        }
        
        public async Task<List<Trade>> GetUserTradesAsync(int userId)
        {
            return await _context.Trades
                .Where(t => t.UserId == userId)
                .Include(t => t.Instrument)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}