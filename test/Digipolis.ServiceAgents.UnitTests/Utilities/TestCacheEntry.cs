using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class TestCacheEntry : ICacheEntry
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; set; }

        public object Key { get; set; }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

        public CacheItemPriority Priority { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public object Value { get; set; }

        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }
}
