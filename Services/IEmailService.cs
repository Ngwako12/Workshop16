using System.Threading.Tasks;

namespace Workshop16.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);

    }
}
