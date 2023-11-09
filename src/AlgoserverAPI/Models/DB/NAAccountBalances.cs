using System;

namespace Algoserver.API.Models
{
    public class NAAccountBalances
    {
        public Guid Id { get; set; }
        public string Account { get; set; }
        public string Currency { get; set; }
        public double Balance { get; set; }
        public double Pnl { get; set; }
        public int AccountType { get; set; }
        public DateTime Date { get; set; }
    }
}
