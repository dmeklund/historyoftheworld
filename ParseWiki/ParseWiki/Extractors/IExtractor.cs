using System.Threading.Tasks;

namespace ParseWiki.Extractors
{
    public interface IExtractor<in T1, T2>
    {
        Task<T2> Extract(T1 block);
    }
}