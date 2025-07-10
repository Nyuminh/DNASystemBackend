using System.Net;
using System.Net.Mail;
using DNASystemBackend.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DNASystemBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string resetCode, string username)
        {
            try
            {
                var smtpSettings = _config.GetSection("EmailSettings");
                var fromEmail = smtpSettings["FromEmail"];
                var fromPassword = smtpSettings["FromPassword"];
                var smtpHost = smtpSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
                    return false;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "DNA System"),
                    Subject = "Mã xác thực đặt lại mật khẩu - DNA System",
                    Body = GetEmailTemplate(username, resetCode),
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, fromPassword)
                };

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                return false;
            }
        }

        private string GetEmailTemplate(string username, string resetCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        .email-container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f8f9fa; }}
        .code-box {{ background-color: #e9ecef; padding: 20px; margin: 20px 0; text-align: center; border-radius: 5px; }}
        .code {{ font-size: 32px; font-weight: bold; color: #007bff; letter-spacing: 3px; }}
        .footer {{ padding: 20px; text-align: center; color: #6c757d; font-size: 12px; }}
        .warning {{ color: #dc3545; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>DNA System</h1>
            <h2>Đặt lại mật khẩu</h2>
        </div>
        <div class='content'>
            <h3>Xin chào {username}!</h3>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản DNA System của mình.</p>
            <p>Sử dụng mã xác thực bên dưới để đặt lại mật khẩu:</p>
            <div class='code-box'>
                <div class='code'>{resetCode}</div>
            </div>
            <p><strong>Lưu ý quan trọng:</strong></p>
            <ul>
                <li>Mã xác thực này sẽ hết hạn sau <strong>30 phút</strong></li>
                <li>Mã chỉ có thể sử dụng <strong>một lần</strong></li>
                <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
            </ul>
            <div class='warning'>
                <p><strong>⚠️ Cảnh báo bảo mật:</strong> Không chia sẻ mã này với bất kỳ ai!</p>
            </div>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống DNA System.</p>
            <p>Nếu bạn cần hỗ trợ, vui lòng liên hệ với chúng tôi.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}