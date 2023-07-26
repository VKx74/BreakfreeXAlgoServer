using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    [Serializable]
    public class AutoTradingInstrumentsResponse
    {
        public string Symbol { get; set; }
        public decimal Risk { get; set; }
    }

    [Serializable]
    public class AutoTradingSymbolInfoResponse
    {
        public float TotalStrength { get; set; }
        public decimal SL { get; set; }
        public decimal HalfBand1M { get; set; }
        public decimal HalfBand5M { get; set; }
        public decimal HalfBand15M { get; set; }
        public decimal HalfBand1H { get; set; }
        public decimal HalfBand4H { get; set; }
        public decimal HalfBand1D { get; set; }
        public decimal Entry1M { get; set; }
        public decimal Entry5M { get; set; }
        public decimal Entry15M { get; set; }
        public decimal Entry1H { get; set; }
        public decimal Entry4H { get; set; }
        public decimal Entry1D { get; set; }
        public decimal TP1M { get; set; }
        public decimal TP5M { get; set; }
        public decimal TP15M { get; set; }
        public decimal TP1H { get; set; }
        public decimal TP4H { get; set; }
        public decimal TP1D { get; set; }
        public decimal Strength1M { get; set; }
        public decimal Strength5M { get; set; }
        public decimal Strength15M { get; set; }
        public decimal Strength1H { get; set; }
        public decimal Strength4H { get; set; }
        public decimal Strength1D { get; set; }
        public decimal AvgOscillator { get; set; }
        public int TrendDirection { get; set; }
        public int TrendState { get; set; }
        public long Time { get; set; }
    }
}
