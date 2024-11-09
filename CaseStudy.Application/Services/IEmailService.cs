using CaseStudy.Application.Common.Email;

namespace CaseStudy.Application.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage emailMessage);
}