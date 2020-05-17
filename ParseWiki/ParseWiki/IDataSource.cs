namespace ParseWiki
{
    public interface IDataSource
    {
        void SaveEvent(int id, string title, string eventtype, DateRange range, Coord coord);
    }
}