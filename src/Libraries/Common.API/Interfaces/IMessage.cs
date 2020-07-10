using Common.Logic.Infrastructure.Interfaces;

namespace Common.API.Interfaces
{
    public interface IMessage
    {
        string Id { get; set; }
        object Data { get; set; }
        long DateTime { get; set; }
        string Description { get; set; }
        IError Error { get; set; }
        bool IsSuccess { get; set; }
        int Total { get; set; }
    }
}
