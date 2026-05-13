using Diploma.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserAuthDto> GetUserByLoginAsync(string login);
        Task<bool> IsUserActiveAsync(string login);
        Task UpdateLastLoginAsync(int userId);
    }
}
