namespace Twelvedata.API.Models.WebSocket
{
    public class TickerMessage : BaseMessage
    {
        public TickerMessage()
        {
            MsgType = nameof(TickerMessage);
        }

        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public string Market { get; set; }
    }
}
