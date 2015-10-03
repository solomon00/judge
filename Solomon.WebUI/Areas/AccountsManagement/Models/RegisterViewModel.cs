using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [StringLength(100, ErrorMessage = "{0} должен быть длинее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Пароль еще раз")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Секретный вопрос")]
        public string SecretQuestion { get; set; }

        [Display(Name = "Секретный ответ")]
        public string SecretAnswer { get; set; }

        [Display(Name = "Необходимо подтверждение через email")]
        public bool Approve { get; set; }

        public bool RequireSecretQuestionAndAnswer { get; set; }
    }
}
