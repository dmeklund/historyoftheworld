using System.Threading.Tasks;

namespace ParseWiki.Sinks
{
    public interface ITitleSink : ISink<string>
    {
        Task SaveTitle(int id, string title);
    }
}