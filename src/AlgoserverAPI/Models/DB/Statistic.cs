using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "decimal(18,3)")]
        public decimal AccountSize { get; set; }
        public string Market { get; set; }
        public string TimeFramePeriodicity { get; set; }
        public int TimeFrameInterval { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal StopLossRatio { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal RiskOverride { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal SplitPositions { get; set; }
    }
}
