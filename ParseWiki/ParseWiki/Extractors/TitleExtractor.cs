using System.Collections.Generic;

namespace ParseWiki.Extractors
{
    public class TitleExtractor : IExtractor<WikiBlock, string>
    {
        public async IAsyncEnumerable<string> Extract(WikiBlock block)
        {
            yield return block.Title;
        }
    }
}