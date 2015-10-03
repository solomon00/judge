using System.ComponentModel.DataAnnotations;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public partial class LoginViewModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }

        public bool EnablePasswordReset { get; set; }
    }
}
