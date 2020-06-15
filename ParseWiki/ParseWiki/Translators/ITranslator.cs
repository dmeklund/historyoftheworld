using System.Threading.Tasks;

namespace ParseWiki.Translators
{
    public interface ITranslator<in T1, T2>
    {
        Task<T2> Translate(T1 item);
    }
}