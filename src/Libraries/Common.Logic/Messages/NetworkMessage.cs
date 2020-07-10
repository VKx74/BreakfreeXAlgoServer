namespace Common.Logic.Messages
{
    public class NetworkMessage<T>
    {
        public string Id { get; set; }
        public T Data { get; set; }
        public long DateTime { get; set; }
        public string Description { get; set; }
        public Error Error { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsDataFull { get; set; }
    }
}
