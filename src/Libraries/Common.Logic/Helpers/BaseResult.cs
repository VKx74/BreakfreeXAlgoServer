using System.Collections.Generic;
using System.Linq;
using Common.Logic.Enums;
using Common.Logic.Infrastructure;
using Common.Logic.Infrastructure.Interfaces;
using Common.Logic.Messages;

namespace Common.Logic.Helpers
{
    public class BaseResult<T> : BaseHelper
    {
        protected static ICollection<T> EmptyCollection()
        {
            return Enumerable.Empty<T>().ToList();
        }

        protected static IOperationResult<bool> Success()
        {
            return new OperationResult<bool>()
            {
                Data = true,
                Error = null,
                IsSuccess = true
            };
        }

        protected static IOperationResult<T> Success(T data = default(T), int total = 0)
        {
            return new OperationResult<T>()
            {
                Data = data,
                Error = null,
                IsSuccess = true,
                Total =  total
            };
        }

        protected static IOperationResult<ICollection<T>> Success(ICollection<T> data, int total = 0)
        {
            return new OperationResult<ICollection<T>>()
            {
                Data = data,
                Error = null,
                IsSuccess = true,
                Total = total
            };
        }

        protected static IOperationResult Fail<TResult>(ErrorType errorType, string source, string description)
        {
            var error = new Error(errorType, source, description);
            return new OperationResult<TResult>()
            {
                Data = default(TResult),
                Error = error,
                IsSuccess = false
            };
        }

        protected static IOperationResult<ICollection<T>>  Fail(ErrorType errorType, string source, string description)
        {
            var error = new Error(errorType, source, description);
            return new OperationResult<ICollection<T>>()
            {
                Data = default(ICollection<T>),
                Error = error,
                IsSuccess = false
            };
        }

        protected static IOperationResult<T> Fail(ErrorType errorType, string source, string description, T data = default(T))
        {
            var error = new Error(errorType, source, description);
            return new OperationResult<T>()
            {
                Data = default(T),
                Error = error,
                IsSuccess = false
            };
        }

        protected static IOperationResult<bool> Fail(ErrorType errorType, string source, string description, bool data = false)
        {
            var error = new Error(errorType, source, description);
            return new OperationResult<bool>()
            {
                Data = data,
                Error = error,
                IsSuccess = false
            };
        }

        //protected static IOperationResult<T> Fail(ErrorType errorType, string source, T data = default(T))
        //{
        //    var description = new Error(errorType, source);
        //    return new OperationResult<T>()
        //    {
        //        Data = default(T),
        //        IsDataFull = true,
        //        Error = description,
        //        IsSuccess = false
        //    };
        //}

        //protected static IOperationResult<ICollection<T>> Fail(ErrorType errorType, string source, ICollection<T> data)
        //{
        //    var description = new Error(errorType, source);
        //    return new OperationResult<ICollection<T>>()
        //    {
        //        Data = data,
        //        IsDataFull = true,
        //        Error = description,
        //        IsSuccess = false
        //    };
        //}

        protected static IOperationResult<T> Fail(IError description, T data = default(T))
        {
            return new OperationResult<T>()
            {
                Data = default(T),
                Error = description,
                IsSuccess = false
            };
        }

        protected static IOperationResult<ICollection<T>> Fail(IError description, ICollection<T> data)
        {
            return new OperationResult<ICollection<T>>()
            {
                Data = data,
                Error = description,
                IsSuccess = false
            };
        }

    }
}
