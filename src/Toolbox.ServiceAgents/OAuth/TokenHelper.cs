using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using Toolbox.ServiceAgents.Models;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.OAuth
{
    public class TokenHelper
    {
        private readonly IMemoryCache _cache;
        private readonly ServiceAgentSettings _options;


        public TokenHelper(IMemoryCache cache, IOptions<ServiceAgentSettings> options)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            if (options == null || options.Value == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            
            _cache = cache;
            _options = options.Value;
            
        }

        public async Task<TokenReply> ReadOrRetrieveToken(string clientID, string clientSecret, string scope, string tokenEndpoint, bool forceNewRetrieval = false)
        {
            TokenReply tokenReplyResult = null;


            if (!forceNewRetrieval)
            {
                //Does it exist in cache???
                tokenReplyResult = _cache.Get<TokenReply>(clientID + clientSecret + scope + tokenEndpoint);
            }

            //Not in cache => retrieve
            if (tokenReplyResult == null)
            {
                tokenReplyResult = await RetrieveToken(clientID, clientSecret, scope, tokenEndpoint);

                //Save in cache for future reference
                int expiration;

                if (int.TryParse(tokenReplyResult.expires_in, out expiration))
                {
                    _cache.Set(clientID + clientSecret + scope + tokenEndpoint, tokenReplyResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 0, expiration) });
                }
            }

            return tokenReplyResult;

        }


        public async Task<TokenReply> RetrieveToken(string clientID, string clientSecret, string scope, string tokenEndpoint)
        {
            TokenReply tokenReply;

            Dictionary<string, string> post = null;
            post = new Dictionary<string, string>
                                {
                                    {"client_id", clientID},
                                    {"client_secret", clientSecret},
                                    { "grant_type", "client_credentials"},
                                    { "scope", scope}
                                };



            using (var client = new HttpClient())
            {
                using (var postContent = new FormUrlEncodedContent(post))
                {
                    //TODO rc01831: errorhandling
                    var response = await client.PostAsync(tokenEndpoint, postContent);
                    var content = await response.Content.ReadAsStringAsync();

                    // received tokens from authorization server
                    tokenReply = JsonConvert.DeserializeObject<TokenReply>(content);

                }
            }

            return tokenReply;
        }
    }

}