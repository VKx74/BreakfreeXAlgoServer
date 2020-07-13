using Twelvedata.API.Services.Instruments;

namespace Twelvedata.API.Models.REST
{
	public class InstrumentExtendedResponse
	{
		public string Symbol { get; set; }

		public string[] AvailableExchanges { get; set; }

		public string CurrencyBase { get; set; }

		public string CurrencyQuote { get; set; }

		public string Name { get; set; }

		public string Exchange { get; set; }

		public string Country { get; set; }

		public string Type { get; set; }

		public InstrumentKind Kind { get; set; }
	}
}
