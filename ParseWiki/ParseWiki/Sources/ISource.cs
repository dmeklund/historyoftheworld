using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParseWiki.Sources
{
    public interface ISource<out T> where T : IWithId
    {
        IAsyncEnumerable<T> FetchAll();
    }
}