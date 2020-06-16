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

namespace ParseWiki.Pipelines
{
    public class NlpEventPipeline
    {
        private class NlpEventExtractor : IExtractor<WikiPageLazyLoadId, WikiEvent>
        {
            private NlpProcessor _proc;
            public NlpEventExtractor()
            {
                _proc = new NlpProcessor();
            }
            
            public async IAsyncEnumerable<WikiEvent> Extract(WikiPageLazyLoadId block)
            {
                var paragraphs = block.Text.Split('\n');
                foreach (var par in paragraphs)
                {
                    var result = await _proc.ProcessText(par);
                    foreach (var sentence in result.sentences)
                    {
                        string location = null;
                        string date = null;
                        if (sentence.entitymentions == null) continue;
                        foreach (var entity in sentence.entitymentions)
                        {
                            if (entity.ner == "LOCATION" || entity.ner == "CITY")
                            {
                                location = entity.text;
                            }
                            else if (entity.ner == "DATE")
                            {
                                date = entity.text;
                            }
                        }

                        if (location != null && date != null)
                        {
                            var eventInfo = sentence.openie.FirstOrDefault(
                                // info => info.subject.Contains(location) || info.object_.Contains(location)
                                info => (
                                    (info.subject.Contains(date) || info.object_.Contains(date)) &&
                                    (info.subject.Contains(location) || info.object_.Contains(location)))
                            );

                            if (eventInfo != null)
                            {
                                // TODO: update coreferences
                                // Console.WriteLine(
                                //     "Found in {0}: {1} {2} {3} ({4})",
                                //     block.Title, 
                                //     eventInfo.subject,
                                //     eventInfo.relation,
                                //     eventInfo.object_,
                                //     date
                                // );
                                yield return new WikiEvent(
                                    $"{eventInfo.subject} {eventInfo.relation} {eventInfo.object_}",
                                    DateRange.Parse(date)
                                );
                                // Console.WriteLine("Original sentence: {0}", sentence);
                            }
                        }
                    }
                }
            }
        }
        
        private readonly ISource<WikiPageLazyLoadId> _source;
        private readonly ISink<WikiEvent> _sink;
        public NlpEventPipeline(ISource<WikiPageLazyLoadId> source, ISink<WikiEvent> sink)
        {
            _source = source;
            _sink = sink;
        }

        public Processor<WikiPageLazyLoadId, WikiEvent> Build()
        {
            var proc = new DataflowProcessor<WikiPageLazyLoadId, WikiEvent>(
                _source,
                new NlpEventExtractor(), 
                _sink
            );
            return proc;
        }
    }
}