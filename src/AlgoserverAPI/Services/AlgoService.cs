using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class AlgoService
    {
        private readonly ILogger<HistoryService> _logger;
        private readonly HistoryService _historyService;


        public AlgoService(ILogger<HistoryService> logger, HistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
