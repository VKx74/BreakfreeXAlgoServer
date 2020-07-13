namespace Algoserver.API.Models.REST
{
    public class CountResponse<T>
    {
        public T Data { get; set; }
        public int Count { get; set; }
    }
}
