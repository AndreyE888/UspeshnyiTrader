using Microsoft.AspNetCore.Mvc;
using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Services;
using Microsoft.AspNetCore.Identity;

namespace UspeshnyiTrader.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionService _sessionService; // ← ЗАМЕНИЛИ на SessionService
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();


        public AccountController(IUserRepository userRepository, ISessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService; // ← ЗАМЕНИЛИ
        }

        [HttpGet]
        public IActionResult Login()
        {
          
            if (_sessionService.IsUserAuthenticated()) // ← ИЗМЕНИЛИ метод
                return RedirectToAction("Profile", "Account");
                
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            Console.WriteLine($"=== ДЕТАЛЬНАЯ ДИАГНОСТИКА ЛОГИНА ===");
            Console.WriteLine($"Username: {username}");

            // Тестируем PasswordHasher (ИСПРАВЛЕНО - используем поле класса)
            var testHash = _passwordHasher.HashPassword(null, "test123");
            Console.WriteLine($"🔍 Test hash for 'test123': {testHash}");
            Console.WriteLine($"🔍 Test verify result: {_passwordHasher.VerifyHashedPassword(null, testHash, "test123")}");

            if (_sessionService.IsUserAuthenticated())
                return RedirectToAction("Index", "Trading");

            var user = await _userRepository.GetByUsernameAsync(username);
            Console.WriteLine($"🔍 User found: {user != null}");

            if (user != null)
            {
                Console.WriteLine($"🔍 DB PasswordHash: {user.PasswordHash}");
                Console.WriteLine($"🔍 DB Hash length: {user.PasswordHash?.Length}");

                // Детальная проверка пароля
                var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, password);
                Console.WriteLine($"🔍 PasswordHasher result: {result}");
                Console.WriteLine($"🔍 Success: {result == PasswordVerificationResult.Success}");

                if (result == PasswordVerificationResult.Success)
                {
                    _sessionService.SetCurrentUserId(user.Id);
                    Console.WriteLine($"✅ УСПЕШНЫЙ ВХОД: {user.Username}");
                    return RedirectToAction("Index", "Trading");
                }
            }

            ViewBag.Error = "Invalid username or password";
            Console.WriteLine($"❌ ОШИБКА ВХОДА для: {username}");
            return View();
        }

        [HttpGet]
        public IActionResult Register() 
        {
            if (_sessionService.IsUserAuthenticated()) // ← ИЗМЕНИЛИ
                return RedirectToAction("Index", "Trading");
                
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (_sessionService.IsUserAuthenticated()) // ← ИЗМЕНИЛИ
                return RedirectToAction("Index", "Trading");

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            if (await _userRepository.UsernameExistsAsync(username))
            {
                ViewBag.Error = "Username already exists";
                return View();
            }

            if (await _userRepository.EmailExistsAsync(email))
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Balance = 1000,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Автоматический вход после регистрации
            _sessionService.SetCurrentUserId(user.Id); // ← ИЗМЕНИЛИ
            return RedirectToAction("Index", "Trading");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Console.WriteLine("=== LOGOUT CALLED ===");
            _sessionService.Logout();
            Console.WriteLine("✅ User logged out successfully");
            return RedirectToAction("Login", "Account");
        }
        
        [HttpGet]
       
        public async Task<IActionResult> Profile()
        {
            var userId = _sessionService.GetCurrentUserId(); // ← ИЗМЕНИЛИ
            if (userId == null)
                return RedirectToAction("Login");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // Методы хеширования пароля (оставляем как были)

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, passwordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}