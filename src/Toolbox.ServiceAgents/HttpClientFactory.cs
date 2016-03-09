using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private IServiceProvider _serviceProvider;
        private HttpClient _client;

        public event Action<IServiceProvider, HttpClient> AfterClientCreated;

        public HttpClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public HttpClient CreateClient(ServiceSettings settings)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.Url)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            switch (settings.AuthScheme)
            {
                case AuthScheme.Bearer:
                    SetBearerAuthHeader();
                    break;
                default:
                    break;
            }

            if (AfterClientCreated != null)
                AfterClientCreated(_serviceProvider, _client);

            return _client;
        }

        private void SetBearerAuthHeader()
        {
            var authContext = _serviceProvider.GetService<IAuthContext>();
            if (authContext == null) throw new NullReferenceException($"{nameof(IAuthContext)} cannot be null.");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthScheme.Bearer, authContext.UserToken);
        }
    }
}
