using System.Threading.Tasks;
using Fintatech.TDS.ClientIdentity.External;
using Microsoft.Extensions.Configuration;

namespace Algoserver.Auth.Services
{
    public class AuthService
    {
        private string _token;
        private readonly ITokenProvider _tokenProvider;
        private readonly AuthorityOptions _authorityOptions;

        public AuthService(ITokenProvider tokenProvider, IConfiguration configuration)
        {
            _tokenProvider = tokenProvider;
            _authorityOptions = new AuthorityOptions();
            configuration.Bind("IdentityOptions", _authorityOptions);
        }

        public async Task<string> GetAuth()
        {
            if (string.IsNullOrEmpty(_token)) {
                var accessToken = await _tokenProvider.GetAccessTokenAsync(_authorityOptions);
                _token = "Bearer " + accessToken;
            }
            return _token;
        }
    }
}