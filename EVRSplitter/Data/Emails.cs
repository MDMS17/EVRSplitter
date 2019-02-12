using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Linq;

namespace EVRSplitter
{
    public class Emails
    {
        public string SMTPServer { get; set; }
        public string sendMessage { get; set; }
        public string sendSubject { get; set; }
        public string sendFrom { get; set; }
        public string[] sendTo { get; set; }
        public bool isHTML { get; set; }
        public List<string> attachments { get; set; }
        public void sendEmail()
        {
            using (MailMessage message = new MailMessage())
            {
                using (SmtpClient client = new SmtpClient(SMTPServer))
                {
                    message.Body = sendMessage;
                    message.Subject = sendSubject;
                    message.From = new MailAddress(sendFrom);
                    foreach (string to in sendTo)
                    {
                        message.To.Add(new MailAddress(to));
                    }
                    if (attachments.Any())
                    {
                        foreach (string attachment in attachments)
                        {
                            message.Attachments.Add(new Attachment(attachment));
                        }
                    }
                    message.IsBodyHtml = isHTML;
                    client.Send(message);
                }
            }
        }
    }
}
