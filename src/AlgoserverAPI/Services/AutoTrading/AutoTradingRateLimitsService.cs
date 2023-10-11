using System.Collections.Generic;
using System.Linq;

namespace Algoserver.API.Services
{
    public class AutoTradingRateLimitsService
    {
        private readonly Dictionary<string, int> _rates = new Dictionary<string, int>();

        public AutoTradingRateLimitsService()
        {

        }

        public bool Validate(string id)
        {
            lock (_rates)
            {
                if (_rates.ContainsKey(id))
                {
                    _rates[id]++;
                    return _rates[id] < 15;
                }

                _rates.Add(id, 1);
            }

            return true;
        }  
        
        public void Decrease()
        {
            lock (_rates)
            {
                var keys = _rates.Keys.Select((_) => _).ToList();
                foreach (var key in keys)
                {
                    _rates[key]--;
                    if (_rates[key] < 0)
                    {
                        _rates.Remove(key);
                    }
                }
            }
        }
        
        public void Clear()
        {
            lock (_rates)
            {
                _rates.Clear();
            }
        }
    }
}