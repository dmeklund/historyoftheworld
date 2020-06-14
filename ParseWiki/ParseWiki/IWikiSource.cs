using System.Collections.Generic;

namespace ParseWiki
{
    public interface IWikiSource
    {
        public IAsyncEnumerable<WikiBlock> FetchAll();
    }
}