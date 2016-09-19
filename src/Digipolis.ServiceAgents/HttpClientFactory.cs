using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.OAuth;

namespace Digipolis.ServiceAgents
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private IServiceProvider _serviceProvider;

        public event Action<IServiceProvider, HttpClient> AfterClientCreated;

        public HttpClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;           
        }

        public HttpClient CreateClient(ServiceAgentSettings serviceAgentSettings, ServiceSettings settings)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(settings.Url)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            switch (settings.AuthScheme)
            {
                case AuthScheme.OAuthClientCredentials:
                    SetOAuthClientCredentialsAuthHeader(client, settings);
                    break;
                case AuthScheme.Bearer:
                    SetBearerAuthHeader(client);
                    break;
                case AuthScheme.ApiKey:
                    SetApiKeyAuthHeader(client, serviceAgentSettings, settings);
                    break;
                default:
                    break;
            }

            if (AfterClientCreated != null)
                AfterClientCreated(_serviceProvider, client);

            return client;
        }

        private void SetApiKeyAuthHeader(HttpClient client, ServiceAgentSettings serviceAgentSettings, ServiceSettings settings)
        {
            if (settings.UseGlobalApiKey)
            {
                client.DefaultRequestHeaders.Add(settings.ApiKeyHeaderName, serviceAgentSettings.GlobalApiKey);
            }
            else
            {
                client.DefaultRequestHeaders.Add(settings.ApiKeyHeaderName, settings.ApiKey);
            }
        }

        private void SetBearerAuthHeader(HttpClient client)
        {
            var authContext = _serviceProvider.GetService<IAuthContext>();
            if (authContext == null) throw new NullReferenceException($"{nameof(IAuthContext)} cannot be null.");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Bearer, authContext.UserToken);
        }

        private void SetOAuthClientCredentialsAuthHeader(HttpClient client, ServiceSettings settings)
        {
            
            var tokenHelper = _serviceProvider.GetService<ITokenHelper>();
            if (tokenHelper == null) throw new NullReferenceException($"{nameof(ITokenHelper)} cannot be null.");
            var token = tokenHelper.ReadOrRetrieveToken(settings).Result.access_token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Bearer, token);
        }


    }
}
