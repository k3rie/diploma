namespace Diploma.Models
{
    public enum UserRole
    {
        Owner = 0,              // Собственник
        DeveloperEngineer = 1,  // Инженер застройщика
        Contractor = 2,         // Подрядчик
        Admin = 3              // Администратор
    }

    public enum DefectStatus
    {
        Created = 0,    // Создан
        Assigned = 1,   // Назначен
        InProgress = 2, // В работе
        Fixed = 3,      // Исправлен
        Confirmed = 4,  // Подтвержден
        Rejected = 5    // Отклонен
    }

    public enum DefectPriority
    {
        Low = 0,        // Низкий
        Medium = 1,     // Средний
        High = 2,       // Высокий
        Critical = 3    // Критический
    }

    public enum CompanyType
    {
        Developer = 0,          // Застройщик
        Contractor = 1,         // Подрядчик
        ManagementCompany = 2   // Управляющая компания
    }
}