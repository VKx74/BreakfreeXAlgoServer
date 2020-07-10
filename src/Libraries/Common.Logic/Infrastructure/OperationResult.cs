using System;
using Common.Logic.Enums;
using Common.Logic.Infrastructure.Interfaces;
using Common.Logic.Messages;

namespace Common.Logic.Infrastructure
{
    public class OperationResult : IOperationResult
    {
        public bool IsSuccess { get; set; }
        public int Total { get; set; }
        public IError Error { get; set; }
        public object Data { get; set; }

        public OperationResult(ErrorType type, string errorSource)
        {
            Error = new Error(type, errorSource);
            Data = null;
            IsSuccess = false;
            Total = 0;
        }
    }

    public class OperationResult<T> : IOperationResult<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public int Total { get; set; }
        public IError Error { get; set; }

        public OperationResult()
        {

        }

        public OperationResult(ErrorType type, string errorSource)
        {
            Error = new Error(type, errorSource);
            Data = default(T);
            IsSuccess = false;
        }

        public OperationResult(ErrorType type, string errorSource, string description)
        {
            Error = new Error(type, errorSource, description);
            Data = default(T);
            IsSuccess = false;
        }

        public OperationResult(ErrorType type, Exception exception)
        {
            Error = new Error(type, exception.Source, exception.Message);
            Data = default(T);
            IsSuccess = false;
        }

        public OperationResult(ErrorType type, string errorSource, string description, object data)
        {
            Error = new Error(type, errorSource, description, data);
            Data = default(T);
            IsSuccess = false;
        }

        public OperationResult(IError description)
        {
            Error = description;
            Data = default(T);
            IsSuccess = false;
        }

        public OperationResult(T data = default(T), int total = 0)
        {
            Data = data;
            IsSuccess = true;
            Total = total;
        }
    }
}
