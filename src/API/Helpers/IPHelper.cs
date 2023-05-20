using System.Net;
using System.Net.Sockets;

namespace API.Helpers;

public static class IPHelper
{
    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = string.Empty;
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
        
            if (remoteIpAddress != null)
            {
                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
        
                if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
        
                return ipAddress;
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "127.0.0.1";
    }
}