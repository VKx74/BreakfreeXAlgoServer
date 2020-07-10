namespace Common.Data.Interfaces
{
    public interface IDbOperationResult
    {
        bool IsSuccess { get; set; }
        bool IsDataFull { get; set; }
        IDbError Error { get; set; }
    }

    public interface IDbOperationResult<T> : IDbOperationResult
    {
        T Data { get; set; }
    }
}
