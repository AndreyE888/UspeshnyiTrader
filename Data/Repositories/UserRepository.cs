using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBalanceAsync(int userId, decimal newBalance)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.Balance = newBalance;
                await UpdateAsync(user);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        
        public async Task<int> GetCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetNewUsersTodayCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Users
                .Where(u => u.CreatedAt.Date == today)
                .CountAsync();
        }

        public async Task<decimal> GetTotalDepositsAsync()
        {
            // Если у пользователя есть поле Balance, считаем общий баланс
            return await _context.Users
                .Where(u => u.IsActive)
                .SumAsync(u => u.Balance);
        }
    }
}