using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task UpdateBalanceAsync(int userId, decimal newBalance);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<int> GetCountAsync();
        
        Task<int> GetNewUsersTodayCountAsync();
        Task<decimal> GetTotalDepositsAsync();
    }
}