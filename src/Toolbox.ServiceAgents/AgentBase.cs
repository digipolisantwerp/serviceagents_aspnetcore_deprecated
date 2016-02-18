using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents
{
    public abstract class AgentBase : IDisposable
    {
        protected readonly ServiceSettings _settings;

        protected readonly JsonMediaTypeFormatter _formatter = new JsonMediaTypeFormatter();
        protected readonly MediaTypeFormatter[] _formatters;

        protected HttpClient _client;
        private IServiceProvider _serviceProvider;

        public AgentBase(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options, string key)
        {
            if (options.Value == null) throw new ArgumentNullException(nameof(ServiceAgentSettings), $"{nameof(ServiceAgentSettings)} cannot be null.");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key), $"{nameof(key)} cannot be null.");
            if (options.Value.Services[key] == null) throw new NullReferenceException($"{nameof(ServiceSettings)} for {key} cannot be null.");

            _serviceProvider = serviceProvider;
            _settings = options.Value.Services[key];

            var clientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _client = clientFactory.CreateClient(_settings);

            _formatters = new MediaTypeFormatter[] { _formatter };
        }

        protected Task<T> ParseResult<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
            return response.Content.ReadAsAsync<T>(_formatters);
        }

        protected void ParseJsonError(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound) throw new NotFoundException();

            var errorJson = response.Content.ReadAsStringAsync().Result;
            try
            {
                dynamic errorObject = JObject.Parse(errorJson);
                throw new HttpRequestException(errorObject?.error?.messages);
            }
            catch (Exception)
            {
                throw new HttpRequestException();
            }
        }



        protected async Task<T> GetAsync<T>(string requestUri)
        { 
            var response = await _client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
            return await response.Content.ReadAsAsync<T>(_formatters);
        }



        protected async Task<T> PostAsync<T>(string requestUri, T item)
        {
            var response = await _client.PostAsync(requestUri, item, _formatter);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PostAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            var response = await _client.PostAsync(requestUri, item, _formatter);
            return await ParseResult<TReponse>(response);
        }



        protected async Task<T> PutAsync<T>(string requestUri, T item)
        {
            var response = await _client.PutAsync(requestUri, item, _formatter);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PutAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            var response = await _client.PutAsync(requestUri, item, _formatter);
            return await ParseResult<TReponse>(response);
        }

        protected async Task PutWithEmptyResultAsync<T>(string requestUri, T item)
        {
            var response = await _client.PutAsync(requestUri, item, _formatter);
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
        }



        protected async Task DeleteAsync(string requestUri)
        {
            var response = await _client.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
        }



        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}