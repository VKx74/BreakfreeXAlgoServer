using Common.Logic.Messages;

namespace BFT.AlgoService.Tests
{
    public class TestMessage
    {
        public string Id { get; set; }
        public object Data { get; set; }
        public long DateTime { get; set; }
        public string Description { get; set; }
        public Error Error { get; set; }
        public bool IsSuccess { get; set; }
        public int Total { get; set; }
    }
}
