using Mvc.Mailer;

namespace Solomon.WebUI.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage RegisterConfirmation(string To, string UserName, string ConfirmationToken);
        MvcMailMessage PasswordReset(string To, string UserName, string ConfirmationToken);
	}
}