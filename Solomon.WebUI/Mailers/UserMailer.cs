using Mvc.Mailer;
using log4net;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;
using log4net.Config;
using System.Web.Mvc;

namespace Solomon.WebUI.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(UserMailer));

		public UserMailer()
		{
			//MasterName = "_Layout";
            XmlConfigurator.Configure();
		}

        public virtual MvcMailMessage RegisterConfirmation(string To, string UserName, string ConfirmationToken)
		{
            //MasterName = "_Layout";
            logger.Info("Begin sending register confirm e-mail to " + UserName + ": " + To + " with confirmation token - " + ConfirmationToken);

            SmtpSection smtp = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

			ViewBag.UserName = UserName;
            ViewBag.ConfirmationToken = ConfirmationToken;

            var message = new MvcMailMessage();

            message.Subject = "Добро пожаловать";
            message.ViewName = "Добро пожаловать";
            message.From = new MailAddress(smtp.From, smtp.From);
            message.To.Add(To);
            
            PopulateBody(message, "RegisterConfirmation", null);
            //var mail = Populate(x =>
            //{
            //    x.MasterName = "_Layout";
            //    x.Subject = "Добро пожаловать";
            //    x.ViewName = "Добро пожаловать";
            //    x.From = new MailAddress(smtp.From, smtp.From);
            //    x.To.Add(To);
            //});

            //return Populate(x =>
            //{
            //    x.MasterName = "_Layout";
            //    x.Subject = "Добро пожаловать";
            //    x.ViewName = "Добро пожаловать";
            //    x.From = new MailAddress(smtp.From, smtp.From);
            //    x.To.Add(To);
            //});

            return message;
		}

        public virtual MvcMailMessage PasswordReset(string To, string UserName, string ConfirmationToken)
        {
            logger.Info("Begin sending reset password e-mail to " + UserName + ": " + To + " with confirmation token - " + ConfirmationToken);

            SmtpSection smtp = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            ViewBag.UserName = UserName;
            ViewBag.ConfirmationToken = ConfirmationToken;

            var message = new MvcMailMessage();

            message.Subject = "Сброс пароля";
            message.ViewName = "Сброс пароля";
            message.From = new MailAddress(smtp.From, smtp.From);
            message.To.Add(To);

            PopulateBody(message, "PasswordReset", null);

            //return Populate(x =>
            //{
            //    x.Subject = "Сброс пароля";
            //    x.ViewName = "Сброс пароля";
            //    x.From = new MailAddress(smtp.From, smtp.From);
            //    x.To.Add(To);
            //});

            return message;
        }
 	}
}