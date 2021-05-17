using System.Net;
using System.Net.Mail;
using appointment_checker.models;

namespace appointment_checker.services
{
    public class EmailNotifier : INotifier
    {
        private readonly string _emailSender;
        private readonly string _emailSenderPassword;
        private readonly string _emailReceiver;
        private readonly string _defaultEmailReceiver;
        private readonly ServicesEnum _context;
        public EmailNotifier(ServicesEnum context, string emailSender, 
                            string emailSenderPassword, string emailReceiver,
                            string defaultEmailReceiver)
        {
            _context = context;
            _emailSender = emailSender;
            _emailSenderPassword = emailSenderPassword;
            _emailReceiver = emailReceiver;
            _defaultEmailReceiver = defaultEmailReceiver;
        }

        public void Notify(Status status, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_emailSender, 
                                                    _emailSenderPassword),
                EnableSsl = true,
            };

            var message = new MailMessage(_emailSender,
                            status == Status.Sucess ? _emailReceiver : _emailSender);
            message.CC.Add(_defaultEmailReceiver);
            message.Subject = $"Appointment Checker {_context.ToString("g")} : {status.ToString("g")}";
            message.Body = $"{_context} response : {body}";
                
            smtpClient.Send(message);
        }
    }
}