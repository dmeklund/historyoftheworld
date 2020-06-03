using System.Threading.Tasks;

namespace ParseWiki
{
    public interface IDataSource
    {
        Task SaveEvent(int id, string title, string eventtype, DateRange range, Coord coord);

        Task SaveLocation(int id, string title, Coord coord);
    }
}