namespace IdentityNetCore.Abstractions
{
    public interface IEmailService
    {
        Task SendEmailAsync(string from, string to, string subject, string body);
    }
}