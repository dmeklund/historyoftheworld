using System.Threading.Tasks;
using ParseWiki.Extractors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

namespace ParseWiki.Processors
{
    public abstract class Processor<T1, T2> where T1 : IWithId
    {
        protected readonly ISource<T1> Source;
        protected readonly IExtractor<T1, T2> Extractor;
        protected readonly ISink<T2> Sink;
        
        internal Processor(ISource<T1> source, IExtractor<T1, T2> extractor, ISink<T2> sink)
        {
            Source = source;
            Extractor = extractor;
            Sink = sink;
        }

        internal abstract Task Process();
    }
}