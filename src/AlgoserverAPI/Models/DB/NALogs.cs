using System;

namespace Algoserver.API.Models
{
    public class NALogs
    {
        public Guid Id { get; set; }
        public string Account { get; set; }
        public string Data { get; set; }
        public int Type { get; set; }
        public DateTime Date { get; set; }
    }
}
