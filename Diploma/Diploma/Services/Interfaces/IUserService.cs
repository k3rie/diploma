using Diploma.Models;
using Diploma.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserAuthDto> GetUserByLoginAsync(string login);
        Task<bool> IsUserActiveAsync(string login);
        Task UpdateLastLoginAsync(int userId);

        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
    }
}