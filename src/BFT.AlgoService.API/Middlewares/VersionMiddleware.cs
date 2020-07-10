using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BFT.AlgoService.API.Middlewares
{
    public class VersionMiddleware
    {
        private readonly string _path;
        private readonly RequestDelegate _next;

        public VersionMiddleware(RequestDelegate next, string path)
        {
            _next = next;
            _path = path;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (IsVersionRequest(httpContext))
            {
                var runtimeVersion = typeof(Startup)
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyFileVersionAttribute>();

                httpContext.Response.Headers.Add("content-type", "application/json");
                await httpContext.Response.WriteAsync($"{{\"version\":\"{runtimeVersion.Version}\"}}");
                return;
            }
            else
            {
                await _next(httpContext);
            }
        }

        private bool IsVersionRequest(HttpContext context)
        {
            return context.Request.Path == _path;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline. 
    public static class VersionMiddlewareExtensions
    {
        public static IApplicationBuilder UseVersionMiddleware(this IApplicationBuilder builder, string path)
        {
            return builder.UseMiddleware<VersionMiddleware>(path);
        }
    }
}
