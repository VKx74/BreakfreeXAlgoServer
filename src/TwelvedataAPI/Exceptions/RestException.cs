using System;
using System.Net;

namespace Algoserver.API.Exceptions
{
    public class RestException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public RestException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public RestException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
