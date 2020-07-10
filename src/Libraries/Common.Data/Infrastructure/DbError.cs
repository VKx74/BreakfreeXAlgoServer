using Common.Data.Helpers;
using Common.Data.Interfaces;
using Common.Data.Enums;

namespace Common.Data.Infrastructure
{
    public class DbError : IDbError
    {
        public DbErrorType ErrorType { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorSource { get; set; }
        public object ErrorData { get; set; }

        public DbError()
        {

        }

        public DbError(DbErrorType errorType, string errorSource)
        {
            ErrorType = errorType;
            ErrorDescription = new EnumHelper().EnumDescription(errorType);
            ErrorSource = errorSource;
        }

        public DbError(DbErrorType errorType, string errorSource, string description)
        {
            ErrorType = errorType;
            ErrorDescription = description;
            ErrorSource = errorSource;
        }

        public DbError(DbErrorType errorType, string errorSource, object data)
        {
            ErrorType = errorType;
            ErrorDescription = new EnumHelper().EnumDescription(errorType);
            ErrorSource = errorSource;
            ErrorData = data;
        }

        public DbError(DbErrorType errorType, string errorSource, string description, object data)
        {
            ErrorType = errorType;
            ErrorDescription = description;
            ErrorSource = errorSource;
            ErrorData = data;
        }
    }
}
