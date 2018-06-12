using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

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

        public HttpClient CreateClient(ServiceSettings settings)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(settings.Url)
            };

            IRequestHeaderHelper requestHeaderHelper = _serviceProvider.GetService<IRequestHeaderHelper>();
            requestHeaderHelper.InitializeHeaders(client, settings);

            // invoke event
            AfterClientCreated?.Invoke(_serviceProvider, client);

            return client;
        }
    }
}