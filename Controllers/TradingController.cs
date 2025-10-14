using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Services;

namespace UspeshnyiTrader.Controllers
{
    public class TradingController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionService _sessionService; // ← ИЗМЕНИТЬ на ISessionService

        public TradingController(IUserRepository userRepository, ISessionService sessionService) // ← ИЗМЕНИТЬ
        {
            _userRepository = userRepository;
            _sessionService = sessionService; // ← ИЗМЕНИТЬ
        }

        public async Task<IActionResult> Index()
        {
            var userId = _sessionService.GetCurrentUserId(); // ← ИЗМЕНИТЬ
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                _sessionService.Logout(); // ← ИЗМЕНИТЬ
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = user.Username;
            ViewBag.UserBalance = user.Balance;
            ViewBag.UserId = user.Id;

            return View();
        }
        
        public async Task<IActionResult> Trade(string symbol)
        {
            var userId = _sessionService.GetCurrentUserId(); // ← ИЗМЕНИТЬ
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                _sessionService.Logout(); // ← ИЗМЕНИТЬ
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Symbol = symbol;
            ViewBag.UserName = user.Username;
            ViewBag.UserBalance = user.Balance;

            return View();
        }
    }
}