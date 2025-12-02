using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public class CandleRepository : ICandleRepository
    {
        private readonly AppDbContext _context;

        public CandleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Candle?> GetByIdAsync(int id)
        {
            return await _context.Candles
                .Include(c => c.Instrument)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Candle>> GetByInstrumentIdAsync(int instrumentId)
        {
            return await _context.Candles
                .Where(c => c.InstrumentId == instrumentId)
                .OrderByDescending(c => c.Time)
                .ToListAsync();
        }

        public async Task<List<Candle>> GetLastCandlesAsync(int instrumentId, int count, TimeSpan interval)
        {
            return await _context.Candles
                .Where(c => c.InstrumentId == instrumentId && c.Interval == interval)
                .OrderByDescending(c => c.Time)
                .Take(count)
                .ToListAsync();
        }
      

        public async Task<Candle?> GetLastCandleAsync(int instrumentId, TimeSpan interval)
        {
            return await _context.Candles
                .Where(c => c.InstrumentId == instrumentId && c.Interval == interval)
                .OrderByDescending(c => c.Time)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Candle candle)
        {
            await _context.Candles.AddAsync(candle);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Candle> candles)
        {
            await _context.Candles.AddRangeAsync(candles);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Candle candle)
        {
            _context.Candles.Update(candle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var candle = await GetByIdAsync(id);
            if (candle != null)
            {
                _context.Candles.Remove(candle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Candles.AnyAsync(c => c.Id == id);
        }
    }
}