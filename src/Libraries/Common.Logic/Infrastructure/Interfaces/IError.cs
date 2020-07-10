using Common.Logic.Enums;

namespace Common.Logic.Infrastructure.Interfaces
{
    public interface IError
    {
        ErrorType ErrorType { get; set; }
        string ErrorDescription { get; set; }
        string ErrorSource { get; set; }
    }
}
