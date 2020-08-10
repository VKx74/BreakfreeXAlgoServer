using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{
    public class TrendResponse {
        public bool isUpTrending { get; set; }
        public decimal hmaValue { get; set; }
    }
    
    public class TrendResponseWithTime: TrendResponse  {
        public long time { get; set; }
    }
    
    public static class TrendDetector {      
        public static TrendResponse Calculate(List<decimal> data, int period = 200)
        {
            var hmaData = TechCalculations.Hma(data, period);
            var last = hmaData.LastOrDefault();

            return new TrendResponse {
                isUpTrending = data.LastOrDefault() >  last,
                hmaValue = last
            };
        } 
        
        public static TrendResponseWithTime[] CalculateRange(List<decimal> data, long[] time)
        {
            var result = new List<TrendResponseWithTime>();
            var hmaData = TechCalculations.Hma(data, 200);

            for (var i = 0; i < hmaData.Count; i++) {
                result.Add(new TrendResponseWithTime {
                    isUpTrending = data[i] > hmaData[i],
                    hmaValue = hmaData[i],
                    time = time[i]
                });
            }

            return result.ToArray();
        }
    }

}
