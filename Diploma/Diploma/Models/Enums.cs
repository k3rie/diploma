using Diploma.Resources;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = nameof(Localization.Created), ResourceType = typeof(Localization))]
        Created = 0,    // Создан
        [Display(Name = nameof(Localization.Assigned), ResourceType = typeof(Localization))]
        Assigned = 1,   // Назначен
        [Display(Name = nameof(Localization.InProgress), ResourceType = typeof(Localization))]
        InProgress = 2, // В работе
        [Display(Name = nameof(Localization.Fixed), ResourceType = typeof(Localization))]
        Fixed = 3,      // Исправлен
        [Display(Name = nameof(Localization.Confirmed), ResourceType = typeof(Localization))]
        Confirmed = 4,  // Подтвержден
        [Display(Name = nameof(Localization.Rejected), ResourceType = typeof(Localization))]
        Rejected = 5    // Отклонен
    }

    public enum DefectPriority
    {
        [Display(Name = nameof(Localization.Low), ResourceType = typeof(Localization))]
        Low = 0,
        [Display(Name = nameof(Localization.Medium), ResourceType = typeof(Localization))]
        Medium = 1,
        [Display(Name = nameof(Localization.High), ResourceType = typeof(Localization))]
        High = 2,
        [Display(Name = nameof(Localization.Critical), ResourceType = typeof(Localization))]
        Critical = 3
    }

    public enum CompanyType
    {
        Developer = 1,          // Застройщик
        Contractor = 2,         // Подрядчик
        ManagementCompany = 3   // Управляющая компания
    }
}