using Diploma.Data.Interfaces;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System.Threading.Tasks;

namespace Diploma.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserAuthDto> GetUserByLoginAsync(string login)
        {
            var user = await _userRepository.GetByUsernameWithCompanyAsync(login);
            if (user == null)
                return null;

            return new UserAuthDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Email = user.Email,
                Phone = user.Phone,
                PasswordHash = user.PasswordHash
            };
        }

        public async Task<bool> IsUserActiveAsync(string login)
        {
            var user = await _userRepository.GetByUsernameAsync(login);
            return user?.IsActive ?? false;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.UpdatedAt = System.DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }
    }
}