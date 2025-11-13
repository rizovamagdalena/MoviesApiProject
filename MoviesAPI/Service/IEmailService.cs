using MoviesAPI.Models.System;

namespace MoviesAPI.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);
    }

}
