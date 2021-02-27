using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParseWiki.Extractors;
using ParseWiki.Sources;
using ParseWiki.Translators;

namespace ParseWiki.DataTypes
{
    public class WikiPageLazyLoadId : IWithId
    {
        private readonly ITranslator<string, long?> _titleToId;
        public string Title { get; }
        public string Text { get; }
        public IDictionary<string, string> Links { get; set; }

        public WikiPageLazyLoadId(string title, string text, ITranslator<string, long?> titleToId)
        {
            Title = title;
            Text = text;
            _titleToId = titleToId;
        }

        public WikiPageLazyLoadId(string title, string text, long id)
        {
            Title = title;
            Text = text;
            _id = id;
            _titleToId = null;
        }

        public async Task InitId()
        {
            _id ??= await _titleToId.Translate(Title);
        }

        public override string ToString()
        {
            return $"{Title} ({Id})";
        }

        private long? _id;
        public long Id => _id.Value;
    }
}