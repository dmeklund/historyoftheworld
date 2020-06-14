using System.Threading.Tasks;

namespace ParseWiki.Sinks
{
    public interface ISink<in T>
    {
        Task Save(int id, T item);
    }
}