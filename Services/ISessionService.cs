namespace UspeshnyiTrader.Services;

public interface ISessionService
{
    void SetCurrentUserId(int userId);
    int? GetCurrentUserId();
    bool IsUserAuthenticated();
    void Logout();
}
