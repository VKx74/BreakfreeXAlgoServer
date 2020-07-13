using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Algoserver.API.Exceptions;

namespace Algoserver.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IActionResultExecutor<ObjectResult> _actionResultExecutor;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, IActionResultExecutor<ObjectResult> actionResultExecutor, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _actionResultExecutor = actionResultExecutor ?? throw new ArgumentNullException(nameof(actionResultExecutor));
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }

            catch (RestException ex)
            {
                await RespondAsync(new ObjectResult(new { Error = ex.Message })
                {
                    StatusCode = (int)ex.StatusCode
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(JsonConvert.SerializeObject(ex));
                await RespondAsync(new BadRequestObjectResult(ex.Message));
            }

            async Task RespondAsync(ObjectResult objectResult) => await SendResponseAsync(context, objectResult);
        }

        private async Task SendResponseAsync(HttpContext context, ObjectResult objectResult)
        {
            await _actionResultExecutor.ExecuteAsync(new ActionContext { HttpContext = context }, objectResult);
        }
    }
}
