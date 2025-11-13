using Microsoft.Extensions.Configuration;
using MoviesAPI.Models.System;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoviesAPI.Service
{
    public class EmailService : IEmailService
    {
        private readonly TransactionalEmailsApi _api;

        public EmailService(IConfiguration configuration)
        {
            var apiKey = configuration["SendinBlue:ApiKey"];
            var config = new Configuration();
            config.ApiKey.Add("api-key", apiKey);
            _api = new TransactionalEmailsApi(config);
        }

        public async System.Threading.Tasks.Task SendEmailAsync(EmailMessage message)
        {
            var sendSmtpEmail = new SendSmtpEmail(
                sender: new SendSmtpEmailSender(name: "Movie App", email: "movietesting27@gmail.com"),
                to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: message.MailTo) },
                subject: message.Subject,
                htmlContent: message.Content
            );

            try
            {
                var result = await _api.SendTransacEmailAsync(sendSmtpEmail);
                Console.WriteLine("Email sent! ID: " + result.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }

}
