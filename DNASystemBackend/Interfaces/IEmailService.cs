namespace DNASystemBackend.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(string toEmail, string resetCode, string username);
    }
}