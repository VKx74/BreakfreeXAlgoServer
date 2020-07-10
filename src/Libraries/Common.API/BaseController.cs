using Common.API.Interfaces;
using Common.Logic.Enums;
using Common.Logic.Infrastructure.Interfaces;
using Common.Logic.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Common.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Common.API
{
    public class BaseController : Controller
    {
        private const string Source = "Base controller";
        private static string _authority;
        protected readonly IService Service;

        public BaseController(IConfigurationRoot config)
        {
            _authority = config.GetValue<string>("auth");
        }

        public BaseController(IService service, IConfigurationRoot config) {
            _authority = config.GetValue<string>("auth");
            Service = service;
        }

        [NonAction]
        protected string GetUserRole()
        {
            var userRole = User.FindFirst("role")?.Value;
            return userRole;
        }


        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //Your logic is here...
            if (Service != null)
                Service.UserId = GetUserId();
        }

        [NonAction]
        protected string GetClientId()
        {
            var clientId = User?.FindFirst("client_id");
            return clientId?.Value;
        }


        [NonAction]
        protected string GetUserId()
        {
            var userId = User.FindFirst("sub")?.Value;
            return userId;
        }

        [NonAction]
        protected static IMessage SuccessResult(object data, int total = 1, string description = "success") => new Message
        {
            Data = data,
            Total = total,
            Description = description,

            IsSuccess = true,
            Error = null,
        };

        [NonAction]
        protected static IMessage ErrorResult(IError description) => new Message
        {
            Data = null,
            Total = 0,
            Description = "Error",

            IsSuccess = false,
            Error = description,
        };

        [NonAction]
        protected static IMessage ErrorResult(ErrorType errorType) => new Message
        {
            Data = null,
            Total = 0,
            Description = "Error",

            IsSuccess = false,
            Error = new Error(errorType, Source),
        };

        [NonAction]
        protected static IMessage ErrorResult(ErrorType errorType, object data) => new Message
        {
            Data = null,
            Total = 0,
            Description = "Error",
            IsSuccess = false,
            Error = new Error(errorType, Source, data),
        };

        [NonAction]
        protected static IMessage InvalidModel() => new Message
        {
            Data = null,
            Total = 0,
            Description = "Error",

            IsSuccess = false,
            Error = new Error(ErrorType.InvalidModel, Source),
        };
    }
}
