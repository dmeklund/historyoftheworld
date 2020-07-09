using System.Threading.Tasks;
using ParseWiki.Extractors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

namespace ParseWiki.Processors
{
    public class SynchronousProcessor<T1,T2> : Processor<T1,T2> where T1 : IWithId
    {
        private ISource<T1> _source;
        private IExtractor<T1, T2> _extractor;
        private ISink<T2> _sink;
        public SynchronousProcessor(ISource<T1> source, IExtractor<T1, T2> extractor, ISink<T2> sink) : base(source, extractor, sink)
        {
            _source = source;
            _extractor = extractor;
            _sink = sink;
        }

        internal override async Task Process()
        {
            await foreach (var input in _source.FetchAll())
            {
                await foreach (var output in _extractor.Extract(input))
                {
                    await _sink.Save(input.Id, output);
                }
            }
        }

        internal override void Cancel()
        {
            throw new System.NotImplementedException();
        }
    }
}