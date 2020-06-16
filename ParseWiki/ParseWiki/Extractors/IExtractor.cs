using System.Collections.Generic;

namespace ParseWiki.Extractors
{
    public interface IExtractor<in T1, T2>
    {
        IAsyncEnumerable<T2> Extract(T1 block);
    }
}