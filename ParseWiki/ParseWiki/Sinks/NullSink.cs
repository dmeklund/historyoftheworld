using System.Threading.Tasks;

namespace ParseWiki.Sinks
{
    public class NullSink<T> : ISink<T>
    {
        public async Task Save(long id, T item)
        {
        }
    }
}