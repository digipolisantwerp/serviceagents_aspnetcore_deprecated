using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents
{
    public class RequestHeaderHelper : IRequestHeaderHelper
    {
        protected IServiceProvider _serviceProvider;
        protected ITokenHelper _tokenHelper = null;

        public RequestHeaderHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeHeaders(HttpClient client, ServiceSettings settings)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            switch (settings.AuthScheme)
            {
                case AuthScheme.OAuthClientCredentials:
                    await SetOAuthClientCredentialsAuthHeader(client, settings);
                    break;
                case AuthScheme.Bearer:
                    SetBearerAuthHeader(client);
                    break;
                case AuthScheme.Basic:
                    SetBasicAuthHeader(client, settings);
                    break;
                default:
                    break;
            }

            if (settings.Headers != null)
            {
                foreach (var header in settings?.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public async Task ValidateAuthHeaders(HttpClient client, ServiceSettings settings)
        {
            switch (settings.AuthScheme)
            {
                case AuthScheme.OAuthClientCredentials:
                    // validate lifetime OAuth access token 
                    await SetOAuthClientCredentialsAuthHeader(client, settings);
                    break;
                case AuthScheme.Bearer:
                    // no change required
                    break;
                case AuthScheme.Basic:
                    // no change required
                    break;
                default:
                    break;
            }
        }

        private void SetBasicAuthHeader(HttpClient client, ServiceSettings settings)
        {
            if (IsDevelopmentEnvironment() == false && settings.Scheme != HttpSchema.Https)
                throw new ServiceAgentException($"Failed to set Basic Authentication header on service agent for host: '{settings.Host}', the actual scheme is '{settings.Scheme}' and should be 'https'!");

            string username = settings.BasicAuthUserName;
            if (!string.IsNullOrEmpty(settings.BasicAuthDomain)) username = $"{settings.BasicAuthDomain}\\{settings.BasicAuthUserName}";
            var headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{settings.BasicAuthPassword}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Basic, headerValue);
        }

        private void SetBearerAuthHeader(HttpClient client)
        {
            var authContext = _serviceProvider.GetService<IAuthContext>();
            if (authContext == null) throw new NullReferenceException($"{nameof(IAuthContext)} cannot be null.");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Bearer, authContext.UserToken);
        }

        private async Task SetOAuthClientCredentialsAuthHeader(HttpClient client, ServiceSettings settings)
        {
            // tokenHelper is only needed for OAuth
            if (_tokenHelper == null)
            {
                _tokenHelper = _serviceProvider.GetService<ITokenHelper>();
                if (_tokenHelper == null) throw new NullReferenceException($"{nameof(ITokenHelper)} cannot be null.");
            }

            var token = await _tokenHelper.ReadOrRetrieveToken(settings);
            var accessToken = token.access_token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Bearer, accessToken);
        }

        private bool IsDevelopmentEnvironment()
        {
            var hostingEnvironment = _serviceProvider.GetRequiredService<IHostingEnvironment>();
            return hostingEnvironment.IsDevelopment();
        }
    }
}