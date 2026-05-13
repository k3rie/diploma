using Diploma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Diploma.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByUsernameWithCompanyAsync(string username);
        Task<User> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string username);
        Task UpdateAsync(User user);
    }
}