using System.Collections.Generic;
using BFT.AlgoService.API.Filters;
using Common.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace BFT.AlgoService.API.Extensions
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var version = AppVersion.FormattedApiVersion;
                c.SwaggerDoc("v1", new Info { Title = "BFT.AlgoService.API " + version, Version = "v1", Description = "Brearfreetrading AlgoService API" });
                
                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                c.AddSecurityRequirement(security);

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    //Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: {apiKey}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                //c.OperationFilter<ApiVersionOperationFilter>();

                //var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                //var xmlPath = Path.Combine(basePath, "docs.xml");
                //c.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            var swaggerRoutePrefix = Program.RoutePrefix;
            var swaggerRoutePrefixForTemplate = swaggerRoutePrefix.StartsWith("/")
                ? swaggerRoutePrefix.RemoveFirst()
                : swaggerRoutePrefix;
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(o => o.RouteTemplate = swaggerRoutePrefixForTemplate + "swagger/{documentName}/swagger.json");
            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "BFT AlgoService Documentation";
                c.RoutePrefix = Program.RoutePrefix.RemoveFirst().RemoveLast() + "/swagger";
                c.SwaggerEndpoint(swaggerRoutePrefix + "swagger/v1/swagger.json", "AlgoService API v1");
                c.DocExpansion(DocExpansion.None);
            });

            return app;
        }
    }
}