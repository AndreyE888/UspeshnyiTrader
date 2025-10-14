using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UspeshnyiTrader.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IUserTokenRepository _tokenRepository;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, IUserTokenRepository tokenRepository)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _tokenRepository = tokenRepository;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return null!;

            // Проверка пароля (используйте ваш PasswordHasher)
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result != PasswordVerificationResult.Success) return null!;

            // Отзываем старые токены пользователя
            await _tokenRepository.RevokeUserTokensAsync(user.Id);

            // Генерируем новый токен
            var token = _jwtService.GenerateToken(user);

            // Сохраняем токен в БД
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _tokenRepository.AddAsync(userToken);

            return token;
        }

        public async Task<bool> LogoutAsync(string token)
        {
            var userToken = await _tokenRepository.GetByTokenAsync(token);
            if (userToken != null)
            {
                userToken.IsRevoked = true;
                await _tokenRepository.UpdateAsync(userToken);
                return true;
            }

            return false;
        }

        public async Task<int?> GetCurrentUserIdAsync(string token)
        {
            // Проверяем в БД сначала
            var userToken = await _tokenRepository.GetValidTokenAsync(token);
            if (userToken == null || userToken.IsRevoked || userToken.ExpiresAt < DateTime.UtcNow)
                return null;

            // Затем проверяем JWT
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            return userId;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var userId = await GetCurrentUserIdAsync(token);
            return userId.HasValue;
        }
    }
}