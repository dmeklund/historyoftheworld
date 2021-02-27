using ParseWiki.Sources;

namespace ParseWiki.DataTypes
{
    public class PageXml : IWithId
    {
        public long Id { get; }
        public string Title { get; }
        public string RawXml { get; }

        public PageXml(long id, string title, string rawXml)
        {
            Id = id;
            Title = title;
            RawXml = rawXml;
        }
    }
}