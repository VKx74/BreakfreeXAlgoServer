namespace Algoserver.API.Models.REST
{
    public class Response<T>
    {
        public T Data { get; set; }

        public Response(T item)
        {
            Data = item;
        }
    }

    public class EncryptedResponse {
        public string data { get; set; }
    }
}
