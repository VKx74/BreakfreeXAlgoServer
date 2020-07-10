using Common.Logic.Enums;
using Common.Logic.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;

namespace Common.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly string _source;
        public GlobalExceptionFilter()
        {
            _source = GetType().Name;
        }

        public void OnException(ExceptionContext context)
        {
            Log.Error(context.Exception.Message + '\n' + context.Exception.Source + '\n' + context.Exception.StackTrace);
            var message = PrepareResponseForException(context.Exception);
            context.ExceptionHandled = true;
            context.Result = new ObjectResult(message)
            {
                StatusCode = 200
            };
        }

        private Message PrepareResponseForException(Exception exception)
        {
            var description = new Error(ErrorType.InvalidOperation, _source, exception.Message);
            return new Message(null, "Error", description);
        }
    }
}
