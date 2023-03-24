using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algoserver.API.Models
{
    public class Backtest
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Result { get; set; }
        public string Parameters { get; set; }
        public string Performance { get; set; }
    }
}
