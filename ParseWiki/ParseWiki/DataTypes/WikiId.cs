using ParseWiki.Sources;

namespace ParseWiki.DataTypes
{
    public class WikiId : IWithId
    {
        public long Id { get; }

        internal WikiId(long id)
        {
            Id = id;
        }
    }
}