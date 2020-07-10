using System;
using Common.API.Interfaces;
using Common.Logic.Infrastructure.Interfaces;

namespace Common.API
{
    public class Message : IMessage
    {
        public string Id { get; set; }
        public object Data { get; set; }
        public long DateTime { get; set; }
        public string Description { get; set; }
        public IError Error { get; set; }
        public bool IsSuccess { get; set; }
        public int Total { get; set; }

        public Message()
        {
            DateTime = (long)(System.DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            Id = Guid.NewGuid().ToString();
        }

        public Message(object data, string description = "", IError error = null, int total = 0) : this()
        {
            Data = data;
            Description = description;
            Error = error;
            IsSuccess = (error == null);
            Total = total;
        }
    }
}
