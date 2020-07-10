using Common.Logic.Enums;
using Common.Logic.Helpers;
using Common.Logic.Infrastructure.Interfaces;

namespace Common.Logic.Messages
{
    public class Error : IError
    {
        public ErrorType ErrorType { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorSource { get; set; }
        public object ErrorData { get; set; }

        public Error()
        {

        }

        public Error(ErrorType errorType, string errorSource)
        {
            ErrorType = errorType;
            ErrorDescription = new EnumHelper().EnumDescription(errorType);
            ErrorSource = errorSource;
        }

        public Error(ErrorType errorType, string errorSource, string description)
        {
            ErrorType = errorType;
            ErrorDescription = description;
            ErrorSource = errorSource;
        }

        public Error(ErrorType errorType, string errorSource, object data)
        {
            ErrorType = errorType;
            ErrorDescription = new EnumHelper().EnumDescription(errorType); ;
            ErrorSource = errorSource;
            ErrorData = data;
        }

        public Error(ErrorType errorType, string errorSource, string description, object data)
        {
            ErrorType = errorType;
            ErrorDescription = description;
            ErrorSource = errorSource;
            ErrorData = data;
        }

        public override string ToString()
        {
            return $"{ErrorType} : {ErrorData?.ToString()} {ErrorDescription} at {ErrorSource}";
        }

    }
}
