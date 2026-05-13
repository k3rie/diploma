using System.ComponentModel.DataAnnotations;

namespace Diploma.Models.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}