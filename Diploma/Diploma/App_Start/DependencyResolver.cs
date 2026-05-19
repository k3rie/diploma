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
        private readonly DefectDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IDefectRepository _defectRepository;
        private readonly IDefectService _defectService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;

        public CustomDependencyResolver()
        {
            _context = new DefectDbContext();

            // Repositories (создаём сначала все репозитории)
            _userRepository = new UserRepository(_context);
            _defectRepository = new DefectRepository(_context);
            _notificationRepository = new NotificationRepository(_context);

            // Services (затем сервисы, передавая нужные зависимости)
            _userService = new UserService(_userRepository);
            _authService = new AuthService(_userService);
            _notificationService = new NotificationService(_notificationRepository);
            _defectService = new DefectService(_defectRepository, _notificationService);
        }

        public object GetService(Type serviceType)
        {
            // DbContext
            if (serviceType == typeof(DefectDbContext))
                return _context;

            // Repositories
            if (serviceType == typeof(IUserRepository))
                return _userRepository;
            if (serviceType == typeof(IDefectRepository))
                return _defectRepository;
            if (serviceType == typeof(INotificationRepository))
                return _notificationRepository;
            // Services
            if (serviceType == typeof(IAuthService))
                return _authService;
            if (serviceType == typeof(IUserService))
                return _userService;
            if (serviceType == typeof(IDefectService))
                return _defectService;

            if (serviceType == typeof(INotificationService))
                return _notificationService;

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            return service != null ? new[] { service } : new object[0];
        }
    }
}