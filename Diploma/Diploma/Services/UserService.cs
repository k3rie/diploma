using Diploma.Data.Interfaces;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
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
        public async Task<List<User>> GetAllUsersAsync() => await _userRepository.GetAllUsersAsync();
        public async Task<User> CreateUserAsync(User user) => await _userRepository.CreateUserAsync(user);
        public async Task DeleteUserAsync(int id) => await _userRepository.DeleteUserAsync(id);
        public async Task<User> GetUserByIdAsync(int id) => await _userRepository.GetByIdAsync(id);
        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }
    }
}