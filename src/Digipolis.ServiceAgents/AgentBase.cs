using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Digipolis.ServiceAgents.Settings;

namespace Digipolis.ServiceAgents
{
    public abstract class AgentBase : IDisposable
    {
        protected readonly ServiceSettings _settings;

        protected readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings();

        protected HttpClient _client;
        private IServiceProvider _serviceProvider;

        public AgentBase(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
        {
            if (options.Value == null) throw new ArgumentNullException(nameof(ServiceAgentSettings), $"{nameof(ServiceAgentSettings)} cannot be null.");

            _serviceProvider = serviceProvider;

            var serviceAgentSettings = options.Value;
            _settings = serviceAgentSettings.Services[this.GetType().Name];

            var clientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _client = clientFactory.CreateClient(serviceAgentSettings, _settings);
        }

        protected async Task<T> ParseResult<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSerializerSettings);
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
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSerializerSettings);
        }

        protected async Task<string> GetStringAsync(string requestUri)
        {
            var response = await _client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) ParseJsonError(response);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<T> PostAsync<T>(string requestUri, T item)
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(requestUri, contentPost);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PostAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(requestUri, contentPost);
            return await ParseResult<TReponse>(response);
        }



        protected async Task<T> PutAsync<T>(string requestUri, T item)
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PutAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
            return await ParseResult<TReponse>(response);
        }

        protected async Task PutWithEmptyResultAsync<T>(string requestUri, T item)
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
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