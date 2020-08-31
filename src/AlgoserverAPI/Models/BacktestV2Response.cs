using System.Collections.Generic;
using Algoserver.API.Models.Broker;

namespace Algoserver.API.Models.REST
{
    
    public class BacktestV2Response 
    {
        public List<Strategy2BacktestSignal> signals { get; set; }
        public List<Order> orders { get; set; }
    }
}
