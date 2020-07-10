using Common.Data.Interfaces;
using System;
using Common.Data.Enums;

namespace Common.Data.Infrastructure
{
    public class DbOperationResult : IDbOperationResult
    {
        public bool IsSuccess { get; set; }
        public bool IsDataFull { get; set; }
        public IDbError Error { get; set; }
        public object Data { get; set; }

       
        public DbOperationResult(DbErrorType type, string errorSource)
        {
            Error = new DbError(type, errorSource);
            Data = null;
            IsSuccess = false;
            IsDataFull = true;
        }
    }

    public class DbOperationResult<T> : IDbOperationResult<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsDataFull { get; set; }
        public IDbError Error { get; set; }

        public DbOperationResult()
        {

        }

        public DbOperationResult(DbErrorType type, string errorSource)
        {
            Error = new DbError(type, errorSource);
            Data = default(T);
            IsSuccess = false;
            IsDataFull = true;
        }

        public DbOperationResult(DbErrorType type, string errorSource, string description)
        {
            Error = new DbError(type, errorSource, description);
            Data = default(T);
            IsSuccess = false;
            IsDataFull = true;
        }

        public DbOperationResult(DbErrorType type, Exception exception)
        {
            Error = new DbError(type, exception.Source, exception.Message);
            Data = default(T);
            IsSuccess = false;
            IsDataFull = true;
        }

        public DbOperationResult(DbErrorType type, string errorSource, string description, object data)
        {
            Error = new DbError(type, errorSource, description, data);
            Data = default(T);
            IsSuccess = false;
            IsDataFull = true;
        }

        public DbOperationResult(IDbError description)
        {
            Error = description;
            Data = default(T);
            IsSuccess = false;
            IsDataFull = true;
        }

        public DbOperationResult(T data = default(T), bool isDataFull = true)
        {
            Data = data;
            IsSuccess = true;
            IsDataFull = isDataFull;
        }

    }
}
