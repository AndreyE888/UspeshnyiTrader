using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Data.Repositories
{
    public interface IUserTokenRepository
    {
        Task<UserToken> GetByTokenAsync(string token);
        Task<UserToken> GetValidTokenAsync(string token);
        Task AddAsync(UserToken token);
        Task UpdateAsync(UserToken token);
        Task RevokeUserTokensAsync(int userId);
    }
}