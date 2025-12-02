using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public class UserBalanceRepository : IUserBalanceRepository
    {
        private readonly AppDbContext _context;

        public UserBalanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserBalance?> GetByIdAsync(int id)
        {
            return await _context.UserBalances
                .Include(ub => ub.User)
                .FirstOrDefaultAsync(ub => ub.Id == id);
        }

        public async Task<List<UserBalance>> GetByUserIdAsync(int userId)
        {
            return await _context.UserBalances
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.Date)
                .ToListAsync();
        }

        public async Task<List<UserBalance>> GetByUserIdAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserBalances
                .Where(ub => ub.UserId == userId && ub.Date >= startDate && ub.Date <= endDate)
                .OrderByDescending(ub => ub.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetUserTotalBalanceAsync(int userId)
        {
            return await _context.UserBalances
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.Date)
                .Select(ub => ub.BalanceAfter)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(UserBalance userBalance)
        {
            await _context.UserBalances.AddAsync(userBalance);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<UserBalance> userBalances)
        {
            await _context.UserBalances.AddRangeAsync(userBalances);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserBalance userBalance)
        {
            _context.UserBalances.Update(userBalance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var userBalance = await GetByIdAsync(id);
            if (userBalance != null)
            {
                _context.UserBalances.Remove(userBalance);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UserBalances.AnyAsync(ub => ub.Id == id);
        }
    }
}