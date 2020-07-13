using Fintatech.TDS.Common.Protocol.BaseMessages.RequestResponse;
using System.ComponentModel.DataAnnotations;

namespace Twelvedata.API.Models.REST
{
    public class HistoryRequest
    {
        public HistoryRequestKind Kind { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Range(1, int.MaxValue)]
        public long Granularity { get; set; }

        public long From { get; set; }

        public long To { get; set; }

        public long BarsCount { get; set; }

        public string Exchange { get; set; }
    }
}
