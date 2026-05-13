

namespace Diploma.Models.DTOs
{
    public class UserAuthDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; } // Добавлено для проверки пароля
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; } // Добавлено
        public string Phone { get; set; } // Добавлено
    }
}