using Digipolis.ServiceAgents.Models;
using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.OAuth
{
    public class TokenHelperTests
    {
        private readonly string _cacheKey;
        private readonly ServiceSettings _settings;
        private IHostingEnvironment _hostingEnv;
        private readonly TestStartup _startup;
        private IApplicationBuilder _app;

        public TokenHelperTests()
        {
            _startup = new TestStartup();

            _cacheKey = "clientIdclientSecretscopehttp://localhost/api/oauth/token";
            _settings = new ServiceSettings
            {
                Scheme = HttpSchema.Http,
                Host = "localhost",
                Path = "api",
                OAuthPathAddition = "oauth/token",
                OAuthClientId = "clientId",
                OAuthClientSecret = "clientSecret",
                OAuthScope = "scope"
            };
        }


        [Fact]
        public void ThrowsExceptionIfCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenHelper(null));
        }

        [Fact]
        public async Task GetTokenFromCache()
        {
            var cache = CreateMockedCache(_cacheKey, new TokenReply { access_token = "accessToken" });
            var tokenHelper = new TokenHelper(cache.Object);

            var token = await tokenHelper.ReadOrRetrieveToken(_settings);

            Assert.Equal("accessToken", token.access_token);
        }

        [Fact]
        public async Task GetToken()
        {
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache(cacheEntry);

            var tokenHelper = new TokenHelper(mockedCache.Object);
            tokenHelper._client = CreateClient();

            var token = await tokenHelper.ReadOrRetrieveToken(_settings);

            Assert.Equal("accessToken", token.access_token);
        }

        [Fact]
        public async Task SetTokenInCache()
        {
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache(cacheEntry);

            var tokenHelper = new TokenHelper(mockedCache.Object);
            tokenHelper._client = CreateClient();

            var token = await tokenHelper.ReadOrRetrieveToken(_settings);

            Assert.Equal("accessToken", ((TokenReply)cacheEntry.Value).access_token);
        }

        [Fact]
        public async Task ThrowsExceptionWhenCallFailed()
        {
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache(cacheEntry);

            var tokenHelper = new TokenHelper(mockedCache.Object);
            tokenHelper._client = CreateClient();

            _settings.OAuthPathAddition = "";

            await Assert.ThrowsAsync<Exception>(async () => await tokenHelper.ReadOrRetrieveToken(_settings));
        }

        [Fact]
        public async Task ThrowsExceptionWhenResponseParsingError()
        {
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache(cacheEntry);

            var tokenHelper = new TokenHelper(mockedCache.Object);
            tokenHelper._client = CreateClient();

            _settings.OAuthPathAddition = "oauth/nocontent";

            await Assert.ThrowsAsync<Exception>(async () => await tokenHelper.ReadOrRetrieveToken(_settings));
        }

        private Mock<IMemoryCache> CreateEmptyMockedCache(TestCacheEntry cacheEntry = null)
        {
            return CreateMockedCache("", null, cacheEntry);
        }

        private Mock<IMemoryCache> CreateMockedCache(string key, TokenReply value, TestCacheEntry cacheEntry = null)
        {
            var mockCache = new Mock<IMemoryCache>();
            var cachedObject = value as object;

            mockCache.Setup(c => c.TryGetValue("", out cachedObject))
                .Returns(false);

            mockCache.Setup(c => c.TryGetValue(key, out cachedObject))
                .Returns(true);

            mockCache.Setup(c => c.CreateEntry(_cacheKey))
                .Returns(cacheEntry);

            return mockCache;
        }

        private TestServer CreateTestServer()
        {
            var hostBuilder = new WebHostBuilder().ConfigureServices(services => _startup.ConfigureServices(services))
                .Configure(app =>
                {
                    _app = app;
                    _hostingEnv = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                    _startup.Configure(app, _hostingEnv);
                });
            return new TestServer(hostBuilder);
        }

        private System.Net.Http.HttpClient CreateClient()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            return client;
        }
    }
}
