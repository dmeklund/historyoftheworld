using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;

using MwParserFromScratch;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public class WikiParser
    {
        private readonly string _filepath;
        private readonly IDataSource _datasource;

        public WikiParser(string filepath, IDataSource datasource)
        {
            this._filepath = filepath;
            this._datasource = datasource;
        } 

        private readonly struct WikiBlock
        {
            public int Id { get; }
            public string Title { get; }
            public string Text { get; }
            public WikiBlock(int id, string title, string text)
            {
                Id = id;
                Title = title;
                Text = text;
            }
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
            var extractor = new TransformBlock<WikiBlock, WikiEvent?>(
                ExtractEvents,
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32 }
            );
            while (await reader.ReadAsync())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "title":
                                isTitle = true;
                                break;
                            case "text":
                                isText = true;
                                break;
                            case "id":
                                isId = true;
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (isTitle)
                        {
                            title = reader.Value;
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
                            if (text.Contains("| coord"))
                            {
                                extractor.Post(new WikiBlock(id, title, text));
                                // ExtractEvents(new WikiBlock(id, title, text));
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

        private WikiEvent? ExtractEvents(WikiBlock block)
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
                var skipDateTypes = new HashSet<String> {"date_format"};
                var astParser = new WikitextParser();
                var ast = astParser.Parse(block.Text);
                DateRange daterange = null;
                foreach (var t in ast.EnumDescendants().OfType<Template>()
                    .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).StartsWith("Infobox")))
                {
                    // string date = String.Empty;
                    var datetype = string.Empty;
                    Coord coord = null;
                    foreach (var attr in t.Arguments)
                    {
                        var attrName = MwParserUtility.NormalizeTemplateArgumentName(attr.Name);
                        if (attrName == null)
                        {
                        }
                        else if (attrName.Contains("coord") && attr.Value.ToString().Trim() != "")
                        {
                            try
                            {
                                coord = Coord.FromWikitext(attr.Value.ToString());
                            }
                            catch (ArgumentException e)
                            {
                                Console.Write(block.Title + ": ");
                                Console.WriteLine(e.Message);
                            }
                        }
                        else if (attrName.Contains("date") && !skipDateTypes.Contains(attrName))
                        {
                            var date = attr.Value.ToPlainText().Trim();
                            daterange = DateRange.Parse(date);
                            // if (daterange == null)
                            //     Console.WriteLine("{0}{1}: Couldn't parse date: {2}", block.Title, dateTypes[datetype], date);
                            datetype = attrName;
                        }
                    }

                    if (daterange != null && coord != null)
                    {
                        var eventType = dateTypes.GetValueOrDefault(datetype, "");
                        Console.WriteLine("{0}{1}: {2}", block.Title, eventType, daterange);
                        _datasource.SaveEvent(block.Id, block.Title, eventType, daterange, coord);
                        return new WikiEvent(block.Title, daterange);
                        // Console.WriteLine("Title: {0}", title);
                        // Console.WriteLine(MwParserUtility.NormalizeTemplateArgumentName(t.Name));
                        // Console.WriteLine("Date ({0}): {1}", datetype, date);
                        // Console.WriteLine("Coord: {0}", coord);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Caught exception during processing: " + e);
            }

            return null;
        }
    }
}
