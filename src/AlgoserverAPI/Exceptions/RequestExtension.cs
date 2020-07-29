using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Algoserver.API.Exceptions
{
    public static class RequestExtension
    {
        public static string ClientIp(this HttpRequest req)
        {
            var forwardedForHeaderValues = req.HttpContext.Request.Headers["X-Forwarded-For"];
            var ipAddress = forwardedForHeaderValues.FirstOrDefault()?.Split(",").FirstOrDefault();

            return string.IsNullOrWhiteSpace(ipAddress)
                ? req.HttpContext.Connection.RemoteIpAddress.ToString()
                : ipAddress;
        }
    }
}
