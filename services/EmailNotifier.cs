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
            _emailReceiver = emailReceiver ?? defaultEmailReceiver;
            _defaultEmailReceiver = defaultEmailReceiver;
        }

        public void Notify(Status status, string subject, string body)
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
            if(_defaultEmailReceiver != null)
            {
                message.CC.Add(_defaultEmailReceiver);
            }
            message.Subject = $"Rendez-vous {_context.ToString("g")} trouvé : {subject}";
            message.Body = $"Lien : {body}";
                
            smtpClient.Send(message);
        }
    }
}