using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParseWiki.Extractors;
using ParseWiki.Sources;

namespace ParseWiki.DataTypes
{
    public class WikiPageLazyLoadId : IWithId
    {
        private readonly IExtractor<string, int?> _idExtractor;
        public string Title { get; }
        public string Text { get; }
        public IDictionary<string, int> Links { get; set; }

        public WikiPageLazyLoadId(string title, string text, IExtractor<string, int?> idExtractor)
        {
            Title = title;
            Text = text;
            _idExtractor = idExtractor;
        }

        public async Task InitId()
        {
            // _idExtractor.Extract(Title)
            _id = await _idExtractor.Extract(Title).FirstOrDefaultAsync();
        }
        
        private int? _id;
        public int Id
        {
            get
            {
                if (_id == null)
                {
                    Console.WriteLine("Warning: ID being initialized synchronously");
                    InitId().Wait();
                }

                return _id.Value;
            }
        }
    }
}