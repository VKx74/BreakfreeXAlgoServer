using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Algoserver.API.Exceptions;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.Instruments;
using Algoserver.Client.REST;
using Algoserver.Client.REST.Requests;

namespace Algoserver.API.Services.History
{
    public class HistoryService
    {
        private readonly ILogger<HistoryService> _logger;
        private readonly InstrumentService _instrumentService;
        private readonly RestClient _restClient;

        public HistoryService(RestClient restClient, ILogger<HistoryService> logger, InstrumentService instrumentService)
        {
            _logger = logger;
            _restClient = restClient;
            _instrumentService = instrumentService;
        }

        public async Task<HistoryResponse> GetHistoryAsync(HistoryRequest request)
        {
            var instrument = _instrumentService.GetInstrumentBySymbol(request.Symbol);
            if (instrument == null)
                throw new RestException(HttpStatusCode.BadRequest, $"Can't find instrument {request.Symbol}");

            if (request.Kind == Fintatech.TDS.Common.Protocol.BaseMessages.RequestResponse.HistoryRequestKind.BarsCount)
            {
                throw new RestException(HttpStatusCode.BadRequest, $"{request.Kind} history request hasn't supported yet");
            }

            var response = new HistoryResponse()
            {
                Symbol = request.Symbol,
                Granularity = request.Granularity,
                Datafeed = Constants.Datafeed,
                Bars = Enumerable.Empty<BarMessage>()
            };

            var twelvedataHistoryRequest = new GetTimeSeriesRequest
            {
                Symbol = request.Symbol,
                Interval = request.Granularity.GetInterval(),
                From = DateTimeOffset.FromUnixTimeSeconds(request.From).DateTime,
                To = DateTimeOffset.FromUnixTimeSeconds(request.To).DateTime
            };

            if (!string.IsNullOrEmpty(request.Exchange)) {
                twelvedataHistoryRequest.Exchange = request.Exchange;
            } else {
                twelvedataHistoryRequest.Exchange = string.Empty;   
            }

            try
            {
                var twelvedataResponse = await _restClient.GetHistoryAsync(twelvedataHistoryRequest);

                if (twelvedataResponse.Values != null && twelvedataResponse.Values.Any())
                {
                    response.Bars = twelvedataResponse.Values.Reverse().Select(b => new BarMessage
                    {
                        Close = b.Close,
                        High = b.High,
                        Low = b.Low,
                        Open = b.Open,
                        Volume = b.Volume,
                        Timestamp = b.Datetime.ToUnixTimeSeconds()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new RestException(HttpStatusCode.BadRequest, "FAILED DURING HISTORY REQUESTING FROM TWELVEDATA API", ex);
            }

            return response;
        }
    }
}
