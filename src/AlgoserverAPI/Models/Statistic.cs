using System;
using System.ComponentModel.DataAnnotations;

namespace Algoserver.API.Models
{
    public class Statistic
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public decimal AccountSize { get; set; }
        public string Market { get; set; }
        public string TimeFramePeriodicity { get; set; }
        public int TimeFrameInterval { get; set; }
        public decimal StopLossRatio { get; set; }
        public decimal RiskOverride { get; set; }
        public decimal SplitPositions { get; set; }
    }
}
