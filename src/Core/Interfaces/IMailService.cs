namespace Core.Interfaces;

public enum MailType
{
    Verify,
    Reset,
    Raw
}
public interface IMailService
{
    Task<bool> SendMail(string receiverEmail, string subject, string body, MailType type);
}