using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Algoserver.API.Data.Repositories;
using Algoserver.API.Helpers;
using Algoserver.API.Models;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    [Serializable]
    public class NALog
    {
        public string data { get; set; }
        public int type { get; set; }
        public long date { get; set; }
    }

    [Serializable]
    public class NALogResponse
    {
        public List<NALog> logs { get; set; }
        public long lastOnlineDate { get; set; }
    }

    public class StatisticsService
    {
        private readonly List<NALogs> _logs;
        private readonly List<NAAccountBalances> _balances;
        private readonly ILogger<StatisticsService> _logger;
        private readonly StatisticsRepository _repo = new StatisticsRepository();

        public StatisticsService(ILogger<StatisticsService> logger)
        {
            _logger = logger;
            _balances = new List<NAAccountBalances>();
            _logs = new List<NALogs>();

            var _logsBatchTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _logsBatchTimer.Elapsed += async (sender, e) => await SaveLogsAndClearCacheAsync();
            _logsBatchTimer.Start();

            var random = new Random();
            var randomInterval = random.Next(0, 60) + 60;
            var _accountsBatchTimer = new Timer(TimeSpan.FromMinutes(randomInterval).TotalMilliseconds);
            _accountsBatchTimer.Elapsed += async (sender, e) => await SaveAccountsAndClearCacheAsync();
            _accountsBatchTimer.Start();
        }

        public void AddLogToCache(NALogs log)
        {
            try
            {
                _logs.Add(log);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during add NA Logs to cache. {ex.Message}");
            }
        }

        public void AddLogsToCache(IEnumerable<NALogs> logs)
        {
            try
            {
                _logs.AddRange(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during add NA Logs to cache. {ex.Message}");
            }
        }

        public async Task<NALogResponse> GetLogs(string account)
        {
            try
            {
                var result = await _repo.GetNALogs(account);
                result = result.OrderByDescending((_) => _.Date).ToList();
                var lastOnlineDateRecord = result.FirstOrDefault((_) => _.Type == 2);
                var logsAndErrors = result.Where((_) => _.Type != 2).Take(100).Select((_) => new NALog {
                    type = _.Type,
                    data = _.Data,
                    date = AlgoHelper.GetUnixTime(_.Date)
                }).ToList();

                return new NALogResponse
                {
                    logs = logsAndErrors,
                    lastOnlineDate = lastOnlineDateRecord != null ? AlgoHelper.GetUnixTime(lastOnlineDateRecord.Date) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during getting NA logs. {ex.Message}");
            }

            return new NALogResponse();
        }

        public void AddAccountToCache(NAAccountBalances acct)
        {
            try
            {
                lock (_balances)
                {
                    _balances.RemoveAll((_) => _.Account == acct.Account);
                    _balances.Add(acct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during add NA Balances to cache. {ex.Message}");
            }
        }

        private async Task SaveLogsAndClearCacheAsync()
        {
            try
            {
                if (_logs.Any())
                {
                    var buffer = _logs.ToArray();
                    _logs.Clear();
                    await _repo.AddNALogs(buffer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during save NA Logs to db. {ex.Message}");
            }
        }

        private async Task SaveAccountsAndClearCacheAsync()
        {
            try
            {
                if (!_balances.Any())
                {
                    return;
                }

                var lastAccount = await _repo.GetLastAccountSaved();

                var buffer = _balances.ToArray();
                _balances.Clear();

                if (lastAccount == null)
                {
                    await _repo.AddNAAccountBalances(buffer);
                }
                else
                {
                    var diff = DateTime.UtcNow - lastAccount.Date;
                    if (diff.TotalHours > 24)
                    {
                        await _repo.AddNAAccountBalances(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during save NA Accounts to db. {ex.Message}");
            }
        }
    }
}
