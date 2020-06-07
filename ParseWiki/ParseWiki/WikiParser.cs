using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using DateLocNLP;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public struct WikiBlock
    {
        public int Id { get; }
        public string Title { get; }
        private Wikitext _wtext;
        private string _text;

        public Wikitext Wtext => _wtext ??= new WikitextParser().Parse(_text);

        public string Text => _text ??= _wtext.ToString();


        public WikiBlock(int id, string title, string text)
        {
            Id = id;
            Title = title;
            _text = text;
            _wtext = null;
        }

        public WikiBlock(int id, string title, Wikitext wtext)
        {
            Id = id;
            Title = title;
            _text = null;
            _wtext = wtext;
        }
    }
    
    public class WikiParser
    {
        private readonly string _filepath;
        private readonly IDataSource _datasource;

        public WikiParser(string filepath, IDataSource datasource)
        {
            this._filepath = filepath;
            this._datasource = datasource;
        }

        private readonly struct WikiEvent
        {
            public DateRange Range { get; }
            public string Title { get; }

            public WikiEvent(string title, DateRange range)
            {
                Title = title;
                Range = range;
            }
        }

        public async Task Parse()
        {
            Console.WriteLine("Parsing {0}", _filepath);
            await using var stream = File.OpenRead(_filepath);
            var settings = new XmlReaderSettings {Async = true};
            using var reader = XmlReader.Create(stream, settings);
            string title = "";
            int id = 0;
            var counter = 0;
            var isTitle = false;
            var isId = false;
            var isText = false;
            // Task<void> task;
            // var extractor = new TransformBlock<WikiBlock, WikiLocation>(
            //     // ExtractEvents,
            //     ExtractLocations,
            //     new ExecutionDataflowBlockOptions
            //     {
            //         MaxDegreeOfParallelism = 32,
            //         BoundedCapacity = 100
            //     }
            // );
            // var saveLocation = new ActionBlock<WikiLocation>(
            //     SaveLocation, 
            //     new ExecutionDataflowBlockOptions
            //     {
            //         MaxDegreeOfParallelism = 5,
            //         BoundedCapacity = 50
            //     }
            // );
            // var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            // extractor.LinkTo(saveLocation, linkOptions);

            var parentElements = new Stack<string>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.EndElement && parentElements.Count != reader.Depth)
                {
                    throw new ApplicationException("Failed to track depth correctly");
                }
                parentElements.TryPeek(out var parentElementName);
                while (GC.GetTotalMemory(false) > 1.5e9)
                {
                    await Task.Delay(1000);
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (parentElementName == "page")
                        {
                            switch (reader.Name)
                            {
                                case "title":
                                    isTitle = true;
                                    break;
                                case "id":
                                    isId = true;
                                    break;
                            }
                        }
                        if (parentElementName == "revision" && reader.Name == "text")
                        {
                            isText = true;
                        }
                        if (!reader.IsEmptyElement)
                        {
                            parentElements.Push(reader.Name);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (isTitle)
                        {
                            title = reader.Value;
                            if (title == "Cincinnati Zoo and Botanical Garden")
                            {
                                Console.WriteLine("Found it!");
                            }
                            isTitle = false;
                        }
                        else if (isId)
                        {
                            id = int.Parse(reader.Value);
                            isId = false;
                        }
                        else if (isText)
                        {
                            isText = false;
                            var text = reader.Value;
                            if (title != "Kid Chocolate")
                            {
                                // await extractor.SendAsync(new WikiBlock(id, title, text));
                                if (text.Contains("coord", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    ThreadPool.QueueUserWorkItem(ExtractLocations, new WikiBlock(id, title, text), true);
                                }
                                // var result = ExtractLocations(new WikiBlock(id, title, text));
                                // await SaveLocation(result);
                            }
                        }
                        break;
                    case XmlNodeType.None:
                        break;
                    case XmlNodeType.Attribute:
                        break;
                    case XmlNodeType.CDATA:
                        break;
                    case XmlNodeType.EntityReference:
                        break;
                    case XmlNodeType.Entity:
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.Document:
                        break;
                    case XmlNodeType.DocumentType:
                        break;
                    case XmlNodeType.DocumentFragment:
                        break;
                    case XmlNodeType.Notation:
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    case XmlNodeType.EndElement:
                        parentElements.Pop();
                        break;
                    case XmlNodeType.EndEntity:
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ++counter;
                // if (counter > 100000)
                    // break;
            }

            // extractor.Complete();
            // await extractor.Completion;
        }
        
        private async void ExtractLocations(WikiBlock block)
        {
            WikiLocation location = null;
            // Console.WriteLine("Parsing {0}", block.Title);
            try
            {
                var parser = new WikitextParser();
                var text = Regex.Replace(block.Text, "<.*?>", "");
                text = text.Replace('<', ' ').Replace('>', ' ');
                var wtext = parser.Parse(text);
                var templates = wtext.EnumDescendants()
                    .OfType<Template>()
                    .Where(t => WikiUtil.NormalizeTemplateName(t.Name)
                        .StartsWith("Infobox", StringComparison.InvariantCultureIgnoreCase));
                foreach (var template in templates)
                {
                    var infobox = Infobox.FromWiki(block, template);
                    if (infobox != null)
                    {
                        location = infobox.ToLocation();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await SaveLocation(location);
        }

        private async Task SaveLocation(WikiLocation location)
        {
            if (location != null)
            {
                Console.WriteLine("Saving location: {0}", location);
                await _datasource.SaveLocation(location.Id, location.Title, location.Coordinate);
            }
        }
        
        private async Task<WikiEvent?> ExtractEvents(WikiBlock block)
        {
            try
            {
                // Console.WriteLine("Title {0} matches", title);
                var dateTypes = new Dictionary<string, string>
                {
                    {"date", ""},
                    {"date_format", ""},
                    {"established_date", " Established"},
                    {"established_date1", " Established"},
                    {"established_date2", " Established"},
                    {"established_date3", " Established"},
                    {"established_date4", " Established"},
                    {"established_date5", " Established"},
                    {"established_date6", " Established"},
                    {"founded_date", " Founded"},
                };
                var skipDateTypes = new HashSet<string> {"date_format"};
                var astParser = new WikitextParser();
                var ast = astParser.Parse(block.Text);
                DateRange daterange = null;
                var proc = new NlpProcessor();
                foreach (var (line, index) in ast.Lines.WithIndex())
                {
                    var sentenceNum = index + 1;
                    var plainText = line.ToPlainText();
                    var result = await proc.ProcessText(plainText);
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
                                Console.WriteLine(
                                    "Found in {0}: {1} {2} {3} ({4})",
                                    block.Title, 
                                    eventInfo.subject,
                                    eventInfo.relation,
                                    eventInfo.object_,
                                    date
                                );
                                Console.WriteLine("Original sentence: {0}", sentence);
                            }
                        }
                    }
                }
                // foreach (var t in ast.EnumDescendants().OfType<Template>()
                //     .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).StartsWith("Infobox")))
                // {
                //     // string date = String.Empty;
                //     var datetype = string.Empty;
                //     Coord coord = null;
                //     foreach (var attr in t.Arguments)
                //     {
                //         var attrName = MwParserUtility.NormalizeTemplateArgumentName(attr.Name);
                //         if (attrName == null)
                //         {
                //         }
                //         else if (attrName.Contains("coord") && attr.Value.ToString().Trim() != "")
                //         {
                //             try
                //             {
                //                 coord = Coord.FromWikitext(attr.Value.ToString());
                //             }
                //             catch (ArgumentException e)
                //             {
                //                 Console.Write(block.Title + ": ");
                //                 Console.WriteLine(e.Message);
                //             }
                //         }
                //         else if (attrName.Contains("date") && !skipDateTypes.Contains(attrName))
                //         {
                //             var date = attr.Value.ToPlainText().Trim();
                //             daterange = DateRange.Parse(date);
                //             // if (daterange == null)
                //             //     Console.WriteLine("{0}{1}: Couldn't parse date: {2}", block.Title, dateTypes[datetype], date);
                //             datetype = attrName;
                //         }
                //     }
                //
                //     if (daterange != null && coord != null)
                //     {
                //         var eventType = dateTypes.GetValueOrDefault(datetype, "");
                //         Console.WriteLine("{0}{1}: {2}", block.Title, eventType, daterange);
                //         _datasource.SaveEvent(block.Id, block.Title, eventType, daterange, coord);
                //         return new WikiEvent(block.Title, daterange);
                //         // Console.WriteLine("Title: {0}", title);
                //         // Console.WriteLine(MwParserUtility.NormalizeTemplateArgumentName(t.Name));
                //         // Console.WriteLine("Date ({0}): {1}", datetype, date);
                //         // Console.WriteLine("Coord: {0}", coord);
                //     }
                // }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Caught exception during processing: " + e);
            }

            return null;
        }
    }
}
