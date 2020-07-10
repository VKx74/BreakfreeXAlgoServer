using Common.Data.Enums;

namespace Common.Data.Interfaces
{
    public interface IDbError
    {
        DbErrorType ErrorType { get; set; }
        string ErrorDescription { get; set; }
        string ErrorSource { get; set; }
    }
}
