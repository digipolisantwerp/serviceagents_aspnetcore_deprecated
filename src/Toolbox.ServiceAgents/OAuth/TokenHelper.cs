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
    public class TokenHelper: ITokenHelper
    {
        private readonly IMemoryCache _cache;


        public TokenHelper(IMemoryCache cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            
            _cache = cache;
            
        }

        public async Task<TokenReply> ReadOrRetrieveToken(ServiceSettings options, bool forceNewRetrieval = false)
        {
            TokenReply tokenReplyResult = null;


            if (!forceNewRetrieval)
            {
                //Does it exist in cache???
                tokenReplyResult = _cache.Get<TokenReply>(options.OAuthClientId + options.OAuthClientSecret + options.OAuthScope + options.OAuthTokenEndpoint);
            }

            //Not in cache => retrieve
            if (tokenReplyResult == null)
            {
                tokenReplyResult = await RetrieveToken(options.OAuthClientId, options.OAuthClientSecret, options.OAuthScope, options.OAuthTokenEndpoint);

                //Save in cache for future reference
                int expiration;

                if (int.TryParse(tokenReplyResult.expires_in, out expiration))
                {
                    _cache.Set(options.OAuthClientId + options.OAuthClientSecret + options.OAuthScope + options.OAuthTokenEndpoint, tokenReplyResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 0, expiration) });
                }
            }

            return tokenReplyResult;

        }


        private async Task<TokenReply> RetrieveToken(string clientID, string clientSecret, string scope, string tokenEndpoint)
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