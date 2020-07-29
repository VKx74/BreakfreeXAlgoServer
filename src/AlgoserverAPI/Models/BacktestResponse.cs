using System.Collections.Generic;
using Algoserver.API.Models.Broker;

namespace Algoserver.API.Models.REST
{
    
    public class BacktestResponse 
    {
        public List<BacktestSignal> signals { get; set; }
        public List<Order> orders { get; set; }
    }
}
