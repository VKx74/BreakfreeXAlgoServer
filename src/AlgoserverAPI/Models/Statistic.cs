using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algoserver.API.Models
{
    public class Statistic
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public string AccountSize { get; set; }
        public string Market { get; set; }
        public string TimeFrame { get; set; }
        public decimal StopLossRatio { get; set; }
        public decimal RiskOverride { get; set; }
        public decimal SplitPositions { get; set; }
    }
    
}
