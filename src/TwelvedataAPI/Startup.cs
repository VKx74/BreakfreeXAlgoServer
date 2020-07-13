using Fintatech.TDS.ClientIdentity.External;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using Twelvedata.API.Conventions;
using Twelvedata.API.Middlewares;
using Twelvedata.API.Services;
using Twelvedata.API.Services.History;
using Twelvedata.API.Services.Instruments;
using Twelvedata.API.Services.Realtime;
using Twelvedata.Client.REST;
using Twelvedata.Client.WebSocket;

namespace Twelvedata.API
{
    public class Startup
    {
        public IConfiguration Configuration { get { return Program.Configuration; } }

        private string RoutePrefix => Configuration.GetValue<string>("RoutePrefix");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //routing prefix stuff
                var routeAttribute = new RouteAttribute(RoutePrefix);
                options.Conventions.Insert(0, new RouteConvention(routeAttribute));
            })
            .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddLogging(opt => opt.AddConsole().AddDebug());

            services.AddSingleton<RealtimeService>();
            services.AddSingleton<UserWebSocketService>();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<HistoryService>();
            services.AddSingleton<InstrumentService>();
            services.AddSingleton<RestClient>(c =>
            {
                var logger = c.GetRequiredService<ILogger<RestClient>>();
                return new RestClient(Configuration["TwelvedataUrls:Rest"], Configuration["ApiKey"], logger);
            });

            services.AddSingleton<TwelveDataWebSocketService>(w =>
            {
                var logger = w.GetRequiredService<ILogger<TwelveDataWebSocketService>>();
                var webSocketUri = Configuration["TwelvedataUrls:WebSocket"];
                var apiKey = Configuration["ApiKey"];
                return new TwelveDataWebSocketService(apiKey, webSocketUri, logger);
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Twelvedata API" });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    In = "header",
                    Type = "apiKey"
                });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "Bearer", new string[] { }
                    }
                };

                options.AddSecurityRequirement(security);
            });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Authority");
                    options.ApiName = Scopes.USER_API;
                    options.RequireHttpsMetadata = false;
                });

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

#if !DEBUG
            app.UseHttpsRedirection();
#endif

            app.UseSwagger(options => options.RouteTemplate = RoutePrefix + "/swagger/{documentName}/swagger.json");
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = RoutePrefix + "/swagger";
                options.SwaggerEndpoint($"/{RoutePrefix}/swagger/v1/swagger.json", "TWELVEDATA API");
                options.DocExpansion(DocExpansion.None);
            });

            app.ApplicationServices.GetRequiredService<InstrumentService>();

            app.UseStaticFiles();
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = Configuration.GetValue<int>("WebSocketBufferSize", 4 * 1024)
            });
            var wsRoute = string.IsNullOrEmpty(RoutePrefix) ? $"/{Routes.SubscribeRealtime}" : $"/{RoutePrefix}/{Routes.SubscribeRealtime}";
            app.Map(wsRoute, builder => builder.UseMiddleware<WebSocketMiddleware>());

            app.UseMvc();
        }
    }
}
