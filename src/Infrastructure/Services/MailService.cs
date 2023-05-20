using Core.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services;

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private const string Email = "";
    private const string Password = "";
    private readonly SmtpClient _smtpClient;
    public MailService(ILogger<MailService> logger)
    {
        _smtpClient = new SmtpClient();
        _logger = logger;
    }

    private MimeMessage WriteMimeMessage(string receiverEmail, string subject, string body)
    {
        var message = new MimeMessage()
        {
            From = { new MailboxAddress(Email, "iCoaching") },
            To = { MailboxAddress.Parse(receiverEmail) },
            Subject = subject,
            Body = new TextPart("html")
            {
                Text = body
            }
        };

        return message;
    }
    private bool Send(MimeMessage message)
    {
        try
        {
            _smtpClient.Connect("smtp.gmail.com", 465, true);
            _smtpClient.Authenticate(Email,Password);
            _smtpClient.Send(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e.StackTrace);
            return false;
        }
        finally
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();
        }
        
        return true;
    }

    public async Task<bool> SendMail(string receiverEmail, string subject, string body, MailType mailType)
    {
        var message = WriteMimeMessage(receiverEmail, subject, body);
        var sendResult = Send(message);
        return sendResult;
    }
}
