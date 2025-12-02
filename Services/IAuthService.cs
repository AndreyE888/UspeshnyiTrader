namespace UspeshnyiTrader.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> LogoutAsync(string token);
        Task<int?> GetCurrentUserIdAsync(string token);
        Task<bool> IsTokenValidAsync(string token);
    }
}