using Digipolis.Errors;
using Digipolis.Errors.Exceptions;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents
{
    public abstract class AgentBase : IDisposable
    {
        protected readonly ServiceSettings _settings;
        protected readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings();

        protected readonly IRequestHeaderHelper _requestHeaderHelper;

        private IServiceProvider _serviceProvider;

        protected HttpClient _client { get; set; }

        protected AgentBase(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
        {
            if (options.Value == null) throw new ArgumentNullException(nameof(ServiceAgentSettings), $"{nameof(ServiceAgentSettings)} cannot be null.");

            _client = client ?? throw new ArgumentNullException(nameof(client), $"Http client cannot be null.");
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), $"Service provider cannot be null.");

            _settings = GetServiceSettings(options.Value);

            _requestHeaderHelper = _serviceProvider.GetService<IRequestHeaderHelper>();
        }

        private ServiceSettings GetServiceSettings(ServiceAgentSettings serviceAgentSettings)
        {
            if (serviceAgentSettings.Services.Any(s => s.Key == GetType().Name))
            {
                return serviceAgentSettings.Services[GetType().Name];
            }

            if (serviceAgentSettings.Services.Any(s => GetType().Name.Contains(s.Key)))
            {
                return serviceAgentSettings.Services.FirstOrDefault(s => GetType().Name.Contains(s.Key)).Value;
            }

            throw new Exception($"Settings not found for service agent {GetType().Name}");
        }

        protected async Task ParseJsonError(HttpResponseMessage response)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            Error errorResponse = null;

            try
            {
                // If there is a response
                if (errorJson.Length > 0)
                {
                    // Try to get Error object from JSON
                    errorResponse = JsonConvert.DeserializeObject<Error>(errorJson, _jsonSerializerSettings);
                    if (errorResponse == null || (string.IsNullOrWhiteSpace(errorResponse.Title) && errorResponse.Status == 0))
                    {
                        // Json couldn't be parsed -> create new error object with custom json
                        throw new Exception();
                    }
                    if (errorResponse.ExtraParameters == null)
                    {
                        errorResponse.ExtraParameters = new Dictionary<string, IEnumerable<string>>();
                    }
                }
            }
            catch (Exception ex)
            {
                OnParseJsonErrorException(ex, response);
                errorResponse = new Error
                {
                    Title = "Json parse error exception.",
                    Status = (int)response.StatusCode,
                    ExtraParameters = new Dictionary<string, IEnumerable<string>> { { "json", new[] { errorJson } }}
                };
            }

            // Throw proper exception based on HTTP status
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new NotFoundException(
                        message: errorResponse?.Title ?? "Not found",
                        code: errorResponse?.Code ?? "NFOUND001",
                        messages: errorResponse?.ExtraParameters);

                case HttpStatusCode.BadRequest:
                    throw new ValidationException(
                        message: errorResponse?.Title ?? "Bad request",
                        code: errorResponse?.Code ?? "UNVALI001",
                        messages: errorResponse?.ExtraParameters);

                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException(
                        message: errorResponse?.Title ?? "Access denied",
                        code: errorResponse?.Code ?? "UNAUTH001",
                        messages: errorResponse?.ExtraParameters);

                case HttpStatusCode.Forbidden:
                    throw new ForbiddenException(
                        message: errorResponse?.Title ?? "Forbidden",
                        code: errorResponse?.Code ?? "FORBID001",
                        messages: errorResponse?.ExtraParameters);

                case HttpStatusCode.BadGateway:
                    throw new BadGatewayException(
                        message: "The server could not be located",
                        code: "GTWAY001",
                        messages: errorResponse?.ExtraParameters);

                case HttpStatusCode.GatewayTimeout:
                    throw new GatewayTimeoutException(
                        message: "The connection to the server timed out",
                        code: "GTWAY002",
                        messages: errorResponse?.ExtraParameters);
                default:
                    throw new ServiceAgentException(
                        message: errorResponse?.Title,
                        code: errorResponse?.Code ?? $"Status: {response.StatusCode}",
                        messages: errorResponse?.ExtraParameters);
            }
        }

        protected virtual void OnParseJsonErrorException(Exception ex, HttpResponseMessage response) { }

        protected async Task<T> GetAsync<T>(string requestUri)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            using (var response = await _client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                return await ParseResult<T>(response);
            }
        }

        protected async Task<string> GetStringAsync(string requestUri)
        {
            using (var response = await GetResponseAsync(requestUri))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                return await StreamToStringAsync(stream) ?? string.Empty;
            }
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(string requestUri)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            var response = await _client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) await ParseJsonError(response);    // only return responses with status code success
            return response;
        }

        protected async Task<T> PostAsync<T>(string requestUri, T item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(requestUri, contentPost);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PostAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(requestUri, contentPost);
            return await ParseResult<TReponse>(response);
        }

        protected async Task<T> PutAsync<T>(string requestUri, T item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PutAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
            return await ParseResult<TReponse>(response);
        }

        protected async Task PutWithEmptyResultAsync<T>(string requestUri, T item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(requestUri, contentPost);
            if (!response.IsSuccessStatusCode)
                await ParseJsonError(response);
        }

        protected async Task DeleteAsync(string requestUri)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            var response = await _client.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                await ParseJsonError(response);
        }

        protected async Task<T> PatchAsync<T>(string requestUri, T item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPatch = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = contentPatch
            };
            var response = await _client.SendAsync(request);
            return await ParseResult<T>(response);
        }

        protected async Task<TReponse> PatchAsync<TRequest, TReponse>(string requestUri, TRequest item)
        {
            await _requestHeaderHelper.ValidateAuthHeaders(_client, _settings);

            HttpContent contentPatch = new StringContent(JsonConvert.SerializeObject(item, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = contentPatch
            };
            var response = await _client.SendAsync(request);
            return await ParseResult<TReponse>(response);
        }

        protected async Task<T> ParseResult<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) await ParseJsonError(response);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                return DeserializeJsonFromStream<T>(stream);
            }
        }

        protected static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        protected static async Task<string> StreamToStringAsync(Stream stream)
        {
            if (stream == null) return null;
            string content;
            using (var streamReader = new StreamReader(stream))
            {
                content = await streamReader.ReadToEndAsync();
            }
            return content;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}