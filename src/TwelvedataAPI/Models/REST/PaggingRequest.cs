namespace Algoserver.API.Models.REST
{
    public class PaggingRequest
    {
        public int Skip { get; set; }

        public int Take { get; set; } = 100;
    }
}
