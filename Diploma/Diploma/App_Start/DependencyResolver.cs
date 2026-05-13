using Diploma.Data;
using Diploma.Data.Interfaces;
using Diploma.Services;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Diploma.App_Start
{
    public class CustomDependencyResolver : IDependencyResolver
    {
        private DefectDbContext _context;
        private IUserRepository _userRepository;
        private IUserService _userService;
        private IAuthService _authService;

        public CustomDependencyResolver()
        {
            _context = new DefectDbContext();
            _userRepository = new UserRepository(_context);
            _userService = new UserService(_userRepository);
            _authService = new AuthService(_userService);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IAuthService))
                return _authService;
            if (serviceType == typeof(IUserService))
                return _userService;
            if (serviceType == typeof(IUserRepository))
                return _userRepository;
            if (serviceType == typeof(DefectDbContext))
                return _context;

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            return service != null ? new[] { service } : new object[0];
        }
    }
}