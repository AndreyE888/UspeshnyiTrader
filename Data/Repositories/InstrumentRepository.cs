using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly AppDbContext _context;

        public InstrumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Instrument?> GetByIdAsync(int id)
        {
            return await _context.Instruments
                .Include(i => i.Candles)
                .Include(i => i.Trades)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Instrument?> GetBySymbolAsync(string symbol)
        {
            return await _context.Instruments
                .FirstOrDefaultAsync(i => i.Symbol == symbol);
        }

        public async Task<List<Instrument>> GetAllAsync()
        {
            return await _context.Instruments
                .Where(i => i.IsActive)
                .OrderBy(i => i.Symbol)
                .ToListAsync();
        }

        public async Task<List<Instrument>> GetActiveAsync()
        {
            return await _context.Instruments
                .Where(i => i.IsActive)
                .OrderBy(i => i.Symbol)
                .ToListAsync();
        }

        public async Task AddAsync(Instrument instrument)
        {
            await _context.Instruments.AddAsync(instrument);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Instrument instrument)
        {
            _context.Instruments.Update(instrument);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePriceAsync(int instrumentId, decimal newPrice)
        {
            var instrument = await GetByIdAsync(instrumentId);
            if (instrument != null)
            {
                instrument.CurrentPrice = newPrice;
                instrument.LastPriceUpdate = DateTime.UtcNow;
                await UpdateAsync(instrument);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var instrument = await GetByIdAsync(id);
            if (instrument != null)
            {
                _context.Instruments.Remove(instrument);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Instruments.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> SymbolExistsAsync(string symbol)
        {
            return await _context.Instruments.AnyAsync(i => i.Symbol == symbol);
        }
        public async Task SaveAllAsync()
        {
            await _context.SaveChangesAsync();
        }
        
    }
}