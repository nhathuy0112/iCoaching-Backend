using System.Text;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services;

public class SendGridMailService : IMailService
{
    private readonly SendGridClient _sendGridClient;
    
    public SendGridMailService(IConfiguration configuration)
    {
        _sendGridClient = new SendGridClient(configuration["SendGrid:API"]);
    }

    private string CreateHtmlContent(string body, MailType mailType)
    {
        var htmlContent = new StringBuilder("");
        switch (mailType)
        {
            case MailType.Reset:
                htmlContent.Append($"<p style=\"font-size: 16px\"><a href=\"{body}\">Bấm vào đây</a> để đặt lại mật khẩu.</p>");
                break;
            case MailType.Verify:
                htmlContent.Append(
                    $"<p style=\"font-size: 16px\"><a href=\"{body}\">Bấm vào đây</a> để xác nhận email.</p>");
                break;
            default:
                htmlContent.Append(body);
                break;
        }

        return htmlContent.ToString();
    }

    public async Task<bool> SendMail(string receiverEmail, string subject, string body, MailType mailType)
    {
        var from = new EmailAddress("notification.icoaching@gmail.com", "iCoaching");
        var to = new EmailAddress(receiverEmail);
        var htmlContent = CreateHtmlContent(body, mailType);
        var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
        var response = await _sendGridClient.SendEmailAsync(message);
        return response.IsSuccessStatusCode;
    }
}