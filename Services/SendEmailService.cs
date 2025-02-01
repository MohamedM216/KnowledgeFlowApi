using System.Net;
using System.Net.Mail;
using KnowledgeFlowApi.Options;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystemAPI.Services.SendEmailServices;

public class SendEmailService(IOptions<EmailOptions> options)
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body)) {
            Console.WriteLine("Invalid email info");
            return;
        }
        try 
        {
            var mailMessage = new MailMessage
            {
                Subject = subject,
                From = new MailAddress(options.Value.SenderEmail),
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(options.Value.SMTPServer, options.Value.SMTPProt)) 
            {
                smtpClient.Credentials = new NetworkCredential(options.Value.SenderEmail, options.Value.SenderPassword);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }

        } catch (Exception ex)
        {
            Console.WriteLine($"ERROR: sending email failed! --- {ex.Message} --- Source: {ex.Source}");
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string username)  {
        string subject = "Welcome to KnowledgeFlowApp!";
        string body = @"
        <html>
            <body>
                <h1>Welcome to KnowledgeFlowApp!</h1>
                <p>Dear " + username + @",</p>
                <p>Thank you for joining Knowledge Flow. We're excited to have you with us.</p>
                <p>...</p>
            </body>
        </html>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
