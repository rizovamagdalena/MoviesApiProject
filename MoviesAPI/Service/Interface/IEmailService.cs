using MoviesAPI.Models.System;

namespace MoviesAPI.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);
    }

}
