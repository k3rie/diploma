using Diploma.Models;
using Diploma.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Diploma.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResultDto> AuthenticateAsync(string login, string password);
        Task<UserAuthDto> GetUserAuthDataAsync(string login);
    }
}