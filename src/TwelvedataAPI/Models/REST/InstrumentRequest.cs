using Algoserver.API.Services.Instruments;

namespace Algoserver.API.Models.REST
{
    public class InstrumentRequest : PaggingRequest
    {
        public string Search { get; set; }
        
        public string Kind { get; set; }

        public string Exchange { get; set; }

        public string Hash() {
            return Search + Kind + Exchange + Skip + Take;
        }
    }
}
