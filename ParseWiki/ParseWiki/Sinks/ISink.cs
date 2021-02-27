using System.Threading.Tasks;

namespace ParseWiki.Sinks
{
    public interface ISink<in T>
    {
        Task Save(long id, T item);
    }
}