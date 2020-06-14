using System.Threading.Tasks;
using ParseWiki.Sinks;

namespace ParseWiki.Extractors
{
    public class TitleExtractor : IExtractor<WikiBlock, string>
    {
        public async Task<string> Extract(WikiBlock block)
        {
            return block.Title;
        }
    }
}