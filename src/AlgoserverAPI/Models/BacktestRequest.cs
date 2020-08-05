namespace Algoserver.API.Models.REST
{
    public class BacktestRequest : CalculationRequest
    {
        public int hma_period { get; set; } = 200;
    }
}
