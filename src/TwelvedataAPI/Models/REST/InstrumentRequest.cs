using Twelvedata.API.Services.Instruments;

namespace Twelvedata.API.Models.REST
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
