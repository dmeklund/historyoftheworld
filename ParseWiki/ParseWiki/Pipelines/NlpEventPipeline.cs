using System.Threading.Tasks;
using DateLocNLP;
using ParseWiki.DataTypes;
using ParseWiki.Extractors;
using ParseWiki.Processors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

namespace ParseWiki.Pipelines
{
    public class NlpEventPipeline
    {
        private class NlpEventExtractor : IExtractor<WikiPageLazyLoadId, WikiEvent>
        {
            private NlpProcessor _proc;
            public NlpEventExtractor()
            {
                _proc = new NlpProcessor();
            }
            public async Task<WikiEvent> Extract(WikiPageLazyLoadId block)
            {
                var paragraphs = block.Text.Split('\n');
                foreach (var par in paragraphs)
                {
                    var result = await _proc.ProcessText(par);
                }
            }
        }
        
        private ISource<WikiPageLazyLoadId> _source;
        private ISink<WikiEvent> _sink;
        public NlpEventPipeline(ISource<WikiPageLazyLoadId> source, ISink<WikiEvent> sink)
        {
            _source = source;
            _sink = sink;
        }

        public Processor<WikiPageLazyLoadId, WikiEvent> Build()
        {
            
        }
    }
}