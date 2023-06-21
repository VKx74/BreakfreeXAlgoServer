using System;

namespace Algoserver.API.Models.EconomicCalendar
{
    [Serializable]
    public class Event
    {
        public string Name { get; set; }
        public object Description { get; set; }
        public string HTMLDescription { get; set; }
        public string InternationalCountryCode { get; set; }
        public string CountryName { get; set; }
        public string EventTypeDescription { get; set; }
        public int? Potency { get; set; }
        public string PotencySymbol { get; set; }
        public string CurrencyId { get; set; }
        public string Symbol { get; set; }
        public bool? IsSpeech { get; set; }
        public bool? IsReport { get; set; }
        public string RiseType { get; set; }
    }

    [Serializable]
    public class EconomicEvent
    {
        public Event Event { get; set; }
        public String DateUtc { get; set; }
        public String ForPeriod { get; set; }
        public int Volatility { get; set; }
        public double? Actual { get; set; }
        public double? Consensus { get; set; }
        public double? Previous { get; set; }
        public double? Revised { get; set; }
        public bool? IsBetterThanExpected { get; set; }
    }
}