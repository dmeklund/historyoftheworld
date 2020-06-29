using System.Collections.Generic;
using DateLocNLP;

namespace ParseWiki.DataTypes
{
    public class WikiEvent
    {
        internal WikiLocation Location { get; }
        internal DateRange Date { get; }
        internal Sentence Sentence { get; }
        internal int PageId { get; }

        public WikiEvent(
            WikiLocation location,
            DateRange date,
            Sentence sentence,
            int pageId
        )
        {
            Location = location;
            Date = date;
            Sentence = sentence;
            PageId = pageId;
        }
    }
}