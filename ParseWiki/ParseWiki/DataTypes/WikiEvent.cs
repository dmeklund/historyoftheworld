namespace ParseWiki.DataTypes
{
    public class WikiEvent
    {
        public DateRange Range { get; }
        public string Title { get; }

        public WikiEvent(string title, DateRange range)
        {
            Title = title;
            Range = range;
        }
    }
}