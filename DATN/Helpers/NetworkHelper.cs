using Microsoft.AspNetCore.Http;
using System.Net;

namespace DATN.Helpers
{
    public static class NetworkHelper
    {
        public static string GetIpAddress(HttpContext context)
        {
            string ipAddress = string.Empty;

            // Kiểm tra header X-Forwarded-For
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                ipAddress = forwardedHeader.Split(',')[0].Trim();
            }

            // Nếu không có X-Forwarded-For, lấy từ connection remote IP
            if (string.IsNullOrEmpty(ipAddress) && context.Connection.RemoteIpAddress != null)
            {
                ipAddress = context.Connection.RemoteIpAddress.ToString();
            }

            // Xử lý IPv4-mapped IPv6 address
            if (ipAddress.Equals("::1", StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = "127.0.0.1";
            }

            if (IPAddress.TryParse(ipAddress, out var parsedIpAddress))
            {
                if (parsedIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    var ipv4Address = parsedIpAddress.MapToIPv4().ToString();
                    if (!string.IsNullOrEmpty(ipv4Address))
                    {
                        ipAddress = ipv4Address;
                    }
                }
            }

            return ipAddress;
        }
    }
} 