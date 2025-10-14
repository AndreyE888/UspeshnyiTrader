using UspeshnyiTrader.Data.Repositories;
using System.Text;
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
        // Сохраняем userId + хеш для проверки
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

        // Проверяем что токен совпадает
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

    private string GenerateSessionToken(int userId)
    {
        // Создаем токен на основе userId + секретного ключа
        var secret = "your-secret-key-change-this"; // Поместите в appsettings.json
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId}-{secret}"));
        return Convert.ToBase64String(hash);
    }
}