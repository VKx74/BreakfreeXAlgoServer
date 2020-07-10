using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BFT.AlgoService.API.Middlewares
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _origins;
        private readonly string _headers;
        private readonly string _methods;
        private readonly string _exposeHeaders;

        public CorsMiddleware(RequestDelegate next, string origins, string headers, string methods, string exposeHeaders)
        {
            _next = next;
            _origins = origins;
            _headers = headers;
            _methods = methods;
            _exposeHeaders = exposeHeaders;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("Access-Control-Allow-Origin", _origins);
            httpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            httpContext.Response.Headers.Add("Access-Control-Allow-Headers", _headers);
            httpContext.Response.Headers.Add("Access-Control-Allow-Methods", _methods);
            httpContext.Response.Headers.Add("Access-Control-Expose-Headers", _exposeHeaders);

            if (httpContext.Request.Method == "OPTIONS")
            {
                // HTTP 204 response code means success,
                // but also that it should expect no content from this (preflight) response
                httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return Task.FromResult(httpContext.Response);
            }

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline. 
    public static class CorsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder, string origins = "*", string headers = "*", string methods = "*", string exposeHeaders = "*")
        {
            return builder.UseMiddleware<CorsMiddleware>(origins, headers, methods, exposeHeaders);
        }
    }
}