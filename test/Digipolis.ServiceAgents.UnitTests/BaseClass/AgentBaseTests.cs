using Digipolis.Errors.Exceptions;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.BaseClass
{
    public class AgentBaseTests : ServiceAgentTestBase
    {
        [Fact]
        public async Task Get()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.GetTestDataAsync();

            Assert.NotNull(response);
            Assert.Equal("Name", response.Name);
            Assert.Equal(150, response.Number);
        }

        [Fact]
        public async Task GetAsString()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.GetTestDataAsStringAsync();

            Assert.NotNull(response);
            Assert.Equal("{\"name\":\"Name\",\"number\":150}", response);
        }

        [Fact]
        public async Task Post()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PostTestDataAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Name);
            Assert.Equal(250, response.Number);
        }

        [Fact]
        public async Task PostWithOtherReturnType()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PostTestDataWithOtherReturnTypeAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Something);
            Assert.Equal(250, response.Id);
        }

        [Fact]
        public async Task Patch()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PatchTestDataAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Name);
            Assert.Equal(250, response.Number);
        }

        [Fact]
        public async Task Put()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PutTestDataAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Name);
            Assert.Equal(250, response.Number);
        }

        [Fact]
        public async Task PutWithEmptyResult()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            await agent.PutWithEmptyResultAsync(new TestModel { Name = "Name3", Number = 350 });

            var sentData = await agent.GetPreviousDataAsync();
            Assert.NotNull(sentData);
            Assert.Equal("Name3", sentData.Name);
            Assert.Equal(350, sentData.Number);
        }

        [Fact]
        public async Task JsonParserErrorWithMissingTitleAndStatusCode()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            var body = JsonConvert.SerializeObject(new
            {
                identifier = "dbcd3004-3af0-4862-bad1-2c4013dec85f",
                extraParameters = (string)null
            });
            message.Content = new StringContent(body);

            var result = await Assert.ThrowsAsync<ServiceAgentException>(async () => await agent.ParseJsonWithError(message));

            Assert.True(result.Messages.Count() == 1);
            var extraParam = result.Messages.FirstOrDefault();

            Assert.NotNull(extraParam);
            Assert.Equal("json", extraParam.Key);
            Assert.True(extraParam.Value.Count() == 1);
            var errorMessage = extraParam.Value.FirstOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal(body, errorMessage);
        }

        [Fact]
        public async Task JsonParserErrorWith1Param()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.Content = new StringContent(@"
            {""identifier"": ""dbcd3004-3af0-4862-bad1-2c4013dec85f"",
               ""title"": ""Client validation failed."",
               ""status"": 400,
               ""extraParameters"": {
                                ""naam"": [""Naam moet uniek zijn"",""Test""],
                                    }
                        }");

            var result = await Assert.ThrowsAsync<ServiceAgentException>(async () => await agent.ParseJsonWithError(message));

            Assert.True(result.Messages.Count() == 1);
            var extraParam = result.Messages.FirstOrDefault();

            Assert.NotNull(extraParam);
            Assert.Equal("naam", extraParam.Key);
            Assert.True(extraParam.Value.Count() == 2);
            var errorMessage = extraParam.Value.FirstOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Naam moet uniek zijn", errorMessage);
            errorMessage = extraParam.Value.LastOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Test", errorMessage);
        }

        [Fact]
        public async Task JsonParserErrorWith2Param()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();

            message.Content = new StringContent(@"
            {""identifier"": ""dbcd3004-3af0-4862-bad1-2c4013dec85f"",
               ""title"": ""Client validation failed."",
               ""status"": 400,
               ""extraParameters"": {
                                ""naam"": [""Naam moet uniek zijn"",""Test""],
                                ""test"": [""Naam moet uniek zijn 2"",""Test2""]
                                    }
                        }");

            var result = await Assert.ThrowsAsync<ServiceAgentException>(async () => await agent.ParseJsonWithError(message));

            Assert.True(result.Messages.Count() == 2);
            var extraParam = result.Messages.FirstOrDefault();
            Assert.NotNull(extraParam);
            Assert.Equal("naam", extraParam.Key);
            Assert.True(extraParam.Value.Count() == 2);
            var errorMessage = extraParam.Value.FirstOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Naam moet uniek zijn", errorMessage);
            extraParam = result.Messages.LastOrDefault();
            Assert.NotNull(extraParam);
            Assert.Equal("test", extraParam.Key);
            Assert.True(extraParam.Value.Count() == 2);
            errorMessage = extraParam.Value.LastOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Test2", errorMessage);
        }

        [Fact]
        public async Task JsonParserError400()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.StatusCode = System.Net.HttpStatusCode.BadRequest;
            message.Content = new StringContent(@"
            {""identifier"": ""dbcd3004-3af0-4862-bad1-2c4013dec85f"",
                ""code"": ""test123"",
               ""title"": ""Client validation failed."",
               ""status"": 401,
               ""extraParameters"": {
                                ""naam"": [""Naam moet uniek zijn"",""Test""],
                                    }
                        }");

            var result = await Assert.ThrowsAsync<ValidationException>(async () => await agent.ParseJsonWithError(message));

            Assert.Equal("Client validation failed.", result.Message);
            Assert.Equal("test123", result.Code);

            Assert.True(result.Messages.Count() == 1);
            var extraParam = result.Messages.FirstOrDefault();

            Assert.NotNull(extraParam);
            Assert.Equal("naam", extraParam.Key);
            Assert.True(extraParam.Value.Count() == 2);
            var errorMessage = extraParam.Value.FirstOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Naam moet uniek zijn", errorMessage);
            errorMessage = extraParam.Value.LastOrDefault();
            Assert.NotNull(errorMessage);
            Assert.Equal("Test", errorMessage);

        }

        [Fact]
        public async Task JsonParserError404()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.Content = new StringContent("");
            message.StatusCode = System.Net.HttpStatusCode.NotFound;

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await agent.ParseJsonWithError(message));

            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonParserError401()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.Content = new StringContent("");
            message.StatusCode = System.Net.HttpStatusCode.Unauthorized;

            var result = await Assert.ThrowsAsync<UnauthorizedException>(async () => await agent.ParseJsonWithError(message));

            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonParserError403()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.Content = new StringContent("");
            message.StatusCode = System.Net.HttpStatusCode.Forbidden;

            var result = await Assert.ThrowsAsync<ForbiddenException>(async () => await agent.ParseJsonWithError(message));

            Assert.NotNull(result);
        }

        [Fact]
        public async Task JsonParserErrorOtherStatus()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            var message = new HttpResponseMessage();
            message.Content = new StringContent(@"<HTML><h1>STATUS 500</h1></HTML>");
            message.StatusCode = HttpStatusCode.InternalServerError;

            var result = await Assert.ThrowsAsync<ServiceAgentException>(async () => await agent.ParseJsonWithError(message));

            Assert.NotNull(result);
            Assert.Equal(await message.Content.ReadAsStringAsync(), result.Messages.FirstOrDefault().Value.FirstOrDefault());
            Assert.Equal(result.Code, $"Status: {HttpStatusCode.InternalServerError.ToString()}");
        }

        [Fact]
        public async Task Delete()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();
            await agent.DeleteAsync();

            var sentData = await agent.GetPreviousDataAsync();
            Assert.NotNull(sentData);
            Assert.Equal("Deleted", sentData.Name);
            Assert.Equal(123, sentData.Number);
        }

        private ServiceAgentSettings CreateServiceAgentSettings()
        {
            var settings = new ServiceAgentSettings();
            settings.Services.Add("TestAgent", new ServiceSettings { Scheme = HttpSchema.Http, Host = "localhost" });
            return settings;
        }

        private IServiceProvider CreateServiceProvider(ServiceAgentSettings settings)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            if (settings != null)
                serviceProviderMock.Setup(p => p.GetService(typeof(IOptions<ServiceAgentSettings>))).Returns(Options.Create(settings));

            var authContextMock = new Mock<IAuthContext>();
            authContextMock.Setup(c => c.UserToken).Returns("TokenValue");

            serviceProviderMock.Setup(p => p.GetService(typeof(IAuthContext))).Returns(authContextMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IHttpClientFactory))).Returns(new HttpClientFactory(serviceProviderMock.Object));


            return serviceProviderMock.Object;
        }
    }
}
