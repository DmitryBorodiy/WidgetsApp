using BetterWidgets.Consts;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace BetterWidgets.Services
{
    public class DelegateAuthenticationProvider : IAuthenticationProvider
    {
        private readonly Func<Task<string>> _acquireToken;

        public DelegateAuthenticationProvider(Func<Task<string>> acquireToken)
        {
            _acquireToken = acquireToken;
        }

        public async Task AuthenticateRequestAsync(
            RequestInformation request, 
            Dictionary<string, object> additionalAuthenticationContext = null, 
            CancellationToken cancellationToken = default)
        {
            var accessToken = await _acquireToken();

            if(string.IsNullOrEmpty(accessToken)) throw new FormatException(string.Format(Errors.JwtTokenIsEmpty));

            request.Headers["Authorization"] = new List<string> { $"Bearer {accessToken}" };
        }
    }
}
