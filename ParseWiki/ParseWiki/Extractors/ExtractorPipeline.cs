using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParseWiki.Extractors
{
    public class ExtractorPipeline<T1,T2,T3> : IExtractor<T1, T3>
    {
        private readonly IExtractor<T1, T2> _ex1;
        private readonly IExtractor<T2, T3> _ex2;
        public ExtractorPipeline(IExtractor<T1, T2> ex1, IExtractor<T2, T3> ex2)
        {
            _ex1 = ex1;
            _ex2 = ex2;
        }

        public async IAsyncEnumerable<T3> Extract(T1 input)
        {
            await foreach (var intermediary in _ex1.Extract(input))
            {
                await foreach (var result in _ex2.Extract(intermediary))
                {
                    yield return result;
                }
            }
        }
    }
}