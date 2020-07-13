namespace Algoserver.Client.WebSocket.Models.Actions
{
    [MessageType("unsubscribe")]
    public class UnsubscribeEvent : TwelvedataAction
    {
        public object Params { get; set; }

        public UnsubscribeEvent(string symbol)
        {
            Params = new
            {
                symbols = symbol
            };
        }
    }
}
