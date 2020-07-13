using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services.Instruments
{

	[DataContract]
	[JsonConverter(typeof(StringEnumConverter))]
	public enum InstrumentKind
	{
		[EnumMember(Value = "Stock")]
		Stock,
		[EnumMember(Value = "Forex")]
		Forex,
		[EnumMember(Value = "Crypto")]
		Crypto,
		[EnumMember(Value = "Indices")]
		Indices,

		[EnumMember(Value = "Unknown")]
		Unknown
	}

	public class Instrument
	{
		public string Symbol { get; set; } = string.Empty;

		public string EscapedSymbol { get; set; } = string.Empty;

		public string Datafeed { get; set; } = string.Empty;

		public string[] AvailableExchanges { get; set; }

		public string CurrencyBase { get; set; } = string.Empty;

		public string CurrencyQuote { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public string Exchange { get; set; } = string.Empty;

		public string Country { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty;

		public InstrumentKind Kind { get; set; }

        [JsonIgnore]
        public string Key => String.IsNullOrWhiteSpace(Exchange) 
            ? Symbol.ToLowerInvariant() 
            : $"{Symbol.ToLowerInvariant()}:{Exchange.ToLowerInvariant()}";

        public InstrumentResponse ToInstrumentResponse() => new InstrumentResponse
		{
			Symbol = Symbol,
			Datafeed = Datafeed
		};

		public InstrumentExtendedResponse ToInstrumentExtendedResponse() => new InstrumentExtendedResponse
		{
			Symbol = Symbol,
			AvailableExchanges = AvailableExchanges,
			Country = Country,
			CurrencyBase = CurrencyBase,
			CurrencyQuote = CurrencyQuote,
			Exchange = Exchange,
			Type = Type,
			Name = Name,
			Kind = Kind
		};

        public override int GetHashCode()
        {
            return new {
                Symbol,
                AvailableExchanges,
                Country,
                CurrencyBase,
                CurrencyQuote,
                Exchange,
                Type,
                Name,
                Kind
            }.GetHashCode() * 17;
        }

        public override bool Equals(object other) => Equals(other as Instrument);

        public bool Equals(Instrument other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Symbol == other.Symbol
                && AvailableExchanges == other.AvailableExchanges
                && Country == other.Country
                && CurrencyBase == other.CurrencyBase
                && CurrencyQuote == other.CurrencyQuote
                && Exchange == other.Exchange
                && Type == other.Type
                && Name == other.Name
                && Kind == other.Kind;
        }
    }
}
