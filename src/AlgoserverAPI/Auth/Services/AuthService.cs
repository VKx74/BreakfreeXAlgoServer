using System;
using System.Threading.Tasks;
using Fintatech.TDS.ClientIdentity.External;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Algoserver.Auth.Services
{
    public class AuthService
    {
        private string _hash = "AuthToken";
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenProvider _tokenProvider;
        private readonly IMemoryCache _cache;
        private readonly AuthorityOptions _authorityOptions;

        public AuthService(ITokenProvider tokenProvider, IConfiguration configuration, IMemoryCache cache, ILogger<AuthService> logger)
        {
            _tokenProvider = tokenProvider;
            _cache = cache;
            _authorityOptions = new AuthorityOptions();
            configuration.Bind("IdentityOptions", _authorityOptions);
            _logger = logger;
        }

        public async Task<string> GetToken()
        {
            try
            {
                if (_cache.TryGetValue(_hash, out string token))
                {
                    return token;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get token from cache");
            }

            var accessToken = await _tokenProvider.GetAccessTokenAsync(_authorityOptions);
            var _token = "Bearer " + accessToken;

            try
            {
                _cache.Set(_hash, _token, TimeSpan.FromMinutes(50));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set token in cache");
            }

            return _token;
        }
    }
}