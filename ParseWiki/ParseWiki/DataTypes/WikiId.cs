using ParseWiki.Sources;

namespace ParseWiki.DataTypes
{
    public class WikiId : IWithId
    {
        public int Id { get; }

        internal WikiId(int id)
        {
            Id = id;
        }
    }
}