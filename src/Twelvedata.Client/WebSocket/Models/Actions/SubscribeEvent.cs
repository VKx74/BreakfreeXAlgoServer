namespace Twelvedata.Client.WebSocket.Models.Actions
{
    [MessageType("subscribe")]
    public class SubscribeEvent : TwelvedataAction
    {   
        public object Params { get; set; }

        public SubscribeEvent(string symbol)
        {
            Params = new
            {
                symbols = symbol
            };
        }
    }
}
