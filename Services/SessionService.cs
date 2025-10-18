using UspeshnyiTrader.Data.Repositories;
using System.Text;
using UspeshnyiTrader.Models.Entities;

namespace UspeshnyiTrader.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public SessionService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public void SetCurrentUserId(int userId)
    {
        var sessionToken = GenerateSessionToken(userId);
        _httpContextAccessor.HttpContext.Session.SetInt32("UserId", userId);
        _httpContextAccessor.HttpContext.Session.SetString("SessionToken", sessionToken);
    }

    public int? GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
        var sessionToken = _httpContextAccessor.HttpContext.Session.GetString("SessionToken");

        if (userId == null || sessionToken == null)
            return null;

        if (sessionToken != GenerateSessionToken(userId.Value))
        {
            Logout();
            return null;
        }

        return userId;
    }

    public bool IsUserAuthenticated()
    {
        return GetCurrentUserId().HasValue;
    }

    public void Logout()
    {
        _httpContextAccessor.HttpContext.Session.Remove("UserId");
        _httpContextAccessor.HttpContext.Session.Remove("SessionToken");
    }

    public async Task<string> GetCurrentUsernameAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue && _userRepository != null)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value);
            return user?.Username ?? "User";
        }
        return "User";
    }

    public async Task<decimal> GetCurrentUserBalanceAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue && _userRepository != null)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value);
            return user?.Balance ?? 0;
        }
        return 0;
    }

    private string GenerateSessionToken(int userId)
    {
        var secret = "your-secret-key-change-this";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId}-{secret}"));
        return Convert.ToBase64String(hash);
    }
}