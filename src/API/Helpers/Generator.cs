namespace API.Helpers;

public class Generator
{
    public static string RandomString()
    {
        var random = new Random();
        var guid = Guid.NewGuid();
        var chars = guid + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" + DateTime.UtcNow.ToShortDateString();
        return new string(
            Enumerable.
                Repeat(chars, 10).
                Select(s => s[random.Next(s.Length)]).ToArray());
    }
}