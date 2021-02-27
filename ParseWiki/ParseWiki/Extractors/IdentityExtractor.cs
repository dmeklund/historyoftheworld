using System.Collections.Generic;

namespace ParseWiki.Extractors
{
    public class IdentityExtractor<T> : IExtractor<T, T>
    {
        public async IAsyncEnumerable<T> Extract(T input)
        {
            yield return input;
        }
    }
}
