namespace Common.Logic.Infrastructure.Interfaces
{
    public interface IOperationResult
    {

        bool IsSuccess { get; set; }
        int Total { get; set; }
        IError Error { get; set; }
    }

    public interface IOperationResult<T> : IOperationResult
    {
        T Data { get; set; }
    }
}
