using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Toolbox.ServiceAgents.Models;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.OAuth
{
    public class TokenHelper : ITokenHelper
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
            TokenReply tokenReply = null;

            var builder = new UriBuilder(tokenEndpoint);


            string query;
            using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                             {
                               new KeyValuePair<string, string>("client_id", clientID),
                               new KeyValuePair<string, string>("client_secret", clientSecret),
                               new KeyValuePair<string, string>("grant_type", "client_credentials"),
                               new KeyValuePair<string, string>("scope", scope),
                             })
                   )
            {
                query = content.ReadAsStringAsync().Result;
            }

            builder.Query = query;

            var stringUri = builder.ToString();

            using (var client = new HttpClient())
            {
                string content;
                try
                {                  
                    var response = await client.PostAsync(stringUri, null);
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {

                    throw new Exception("Error retrieving token: " + ex.Message, ex);
                }

                // received tokens from authorization server
                if (content != null)
                {
                    try
                    {
                        tokenReply = JsonConvert.DeserializeObject<TokenReply>(content);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error parsing token: " + ex.Message, ex);
                    }
                }
            }

            return tokenReply;
        }
    }

}