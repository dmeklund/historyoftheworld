using System.Threading.Tasks;

namespace ParseWiki
{
    public interface IDataSource
    {
        Task SaveEvent(long id, string title, string eventtype, DateRange range, Coord coord);

        Task SaveLocation(long id, string title, Coord coord);
    }
}