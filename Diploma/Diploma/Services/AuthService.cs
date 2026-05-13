using BCrypt.Net;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System.Threading.Tasks;

namespace Diploma.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<LoginResultDto> AuthenticateAsync(string login, string password)
        {
            var user = await _userService.GetUserByLoginAsync(login);

            if (user == null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            // Для BCrypt.Net-Next используем BCrypt.Verify()
            bool isValidPassword =BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isValidPassword)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            await _userService.UpdateLastLoginAsync(user.Id);

            return new LoginResultDto
            {
                Success = true,
                User = user
            };
        }

        public async Task<UserAuthDto> GetUserAuthDataAsync(string login)
        {
            return await _userService.GetUserByLoginAsync(login);
        }

    }
}