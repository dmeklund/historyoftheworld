using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DateLocNLP;
using ParseWiki.DataTypes;
using ParseWiki.Extractors;
using ParseWiki.Processors;
using ParseWiki.Sinks;
using ParseWiki.Sources;
using ParseWiki.Translators;
using Microsoft.EntityFrameworkCore;

namespace ParseWiki.Pipelines
{
    public class NlpEventPipeline
    {
        private class NlpEventExtractor : IExtractor<WikiPageLazyLoadId, WikiEvent>
        {
            private readonly NlpProcessor _proc;
            private readonly ITranslator<int, WikiLocation> _idToLocation;
            private readonly ITranslator<string, WikiLocation> _titleToLocation;
            private readonly ITranslator<string, int?> _titleToId;
            public NlpEventExtractor(
                ITranslator<int,WikiLocation> idToLocation, 
                ITranslator<string,WikiLocation> titleToLocation,
                ITranslator<string,int?> titleToId
            )
            {
                _proc = new NlpProcessor();
                _idToLocation = idToLocation;
                _titleToLocation = titleToLocation;
                _titleToId = titleToId;
            }
            
            public async IAsyncEnumerable<WikiEvent> Extract(WikiPageLazyLoadId block)
            {
                var paragraphs = block.Text.Split('\n');
                // var paragraphs = new[] {block.Text};
                foreach (var par in paragraphs)
                {
                    var result = await _proc.ProcessText(par);
                    foreach (var sentence in result.sentences)
                    {
                        var locations = new List<WikiLocation>();
                        var dates = new List<DateRange>();
                        if (sentence.entitymentions == null) continue;
                        foreach (var entity in sentence.entitymentions)
                        {
                            if (entity.ner == "LOCATION" || entity.ner == "CITY")
                            {
                                var location = await FindLocation(entity.text, block.Links);
                                if (location != null)
                                {
                                    locations.Add(location);
                                }
                            }
                            else if (entity.ner == "DATE")
                            {
                                var date = DateRange.Parse(entity.text);
                                if (date != null)
                                {
                                    dates.Add(date);
                                }
                            }
                        }

                        if (locations.Count > 0 && dates.Count == 1)
                        {
                            if (sentence.openie.Count > 0)
                            {
                                sentence.LinkCoreferences(result.corefs.Values);
                                await block.InitId();
                                Console.WriteLine($"Passing along event for sentence: {sentence}");
                                yield return new WikiEvent(
                                    locations[0], dates[0], sentence, block.Id
                                );
                            }
                        }
                    }
                }
            }
            

            private async Task<WikiLocation> FindLocation(string name, IDictionary<string, string> links)
            {
                WikiLocation location;
                if (links.TryGetValue(name, out var title))
                {
                    var id = await _titleToId.Translate(title);
                    if (id != null)
                    {
                        location = await _idToLocation.Translate(id.Value);
                        if (location != null)
                        {
                            return location;
                        }
                    }
                }
                location = await _titleToLocation.Translate(name);
                return location;
            }
        }
        
        private readonly ISource<WikiPageLazyLoadId> _source;
        private readonly ISink<WikiEvent> _sink;
        private readonly ITranslator<int, WikiLocation> _idToLocation;
        private readonly ITranslator<string, WikiLocation> _titleToLocation;
        private readonly ITranslator<string, int?> _titleToId;
        public NlpEventPipeline(
            ISource<WikiPageLazyLoadId> source,
            ISink<WikiEvent> sink,
            ITranslator<int, WikiLocation> idToLocation,
            ITranslator<string, WikiLocation> titleToLocation,
            ITranslator<string, int?> titleToId
        )
        {
            _source = source;
            _sink = sink;
            _idToLocation = idToLocation;
            _titleToLocation = titleToLocation;
            _titleToId = titleToId;
        }

        public Processor<WikiPageLazyLoadId, WikiEvent> Build()
        {
            var proc = new DataflowProcessor<WikiPageLazyLoadId, WikiEvent>(
                _source,
                new NlpEventExtractor(_idToLocation, _titleToLocation, _titleToId), 
                _sink
            );
            return proc;
        }
    }
}