using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Digipolis.ServiceAgents
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private IServiceProvider _serviceProvider;
        private IRequestHeaderHelper _requestHeaderHelper;

        public event Action<IServiceProvider, HttpClient> AfterClientCreated;

        public HttpClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _requestHeaderHelper = serviceProvider.GetService<IRequestHeaderHelper>();
        }

        public HttpClient CreateClient(ServiceSettings settings)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(settings.Url)
            };

            _requestHeaderHelper.InitializeHeaders(client, settings);

            // invoke event
            AfterClientCreated?.Invoke(_serviceProvider, client);

            return client;
        }
    }
}