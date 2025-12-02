using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface IUserBalanceRepository
    {
          Task<UserBalance?> GetByIdAsync(int id);
        Task<List<UserBalance>> GetByUserIdAsync(int userId);
        Task<List<UserBalance>> GetByUserIdAsync(int userId, DateTime startDate, DateTime endDate);
              Task<decimal> GetUserTotalBalanceAsync(int userId);
        Task AddAsync(UserBalance userBalance);
        Task AddRangeAsync(IEnumerable<UserBalance> userBalances);
        Task UpdateAsync(UserBalance userBalance);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}