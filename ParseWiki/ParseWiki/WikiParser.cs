using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using MwParserFromScratch;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public class WikiParser
    {
        private readonly string _filepath;

        public WikiParser(string filepath) => this._filepath = filepath;

        public void Parse()
        {
            Console.WriteLine("Parsing {0}", _filepath);
            using var stream = File.OpenRead(_filepath);
            using var reader = XmlReader.Create(stream);
            string title = "";
            var counter = 0;
            var isTitle = false;
            var isText = false;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "title")
                        {
                            isTitle = true;
                        }
                        else if (reader.Name == "text")
                        {
                            isText = true;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (isTitle)
                        {
                            title = reader.Value;
                            isTitle = false;
                        }
                        else if (isText)
                        {
                            isText = false;
                            var text = reader.Value;
                            if (text.Contains("\n| coord") && text.Contains("\n| date "))
                            {
                                ExtractEvents(text, title);
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
        }

        private static void ExtractEvents(string text, string title)
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
            var ast = astParser.Parse(text);
            foreach (var t in ast.EnumDescendants().OfType<Template>()
                .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).StartsWith("Infobox")))
            {
                string date = String.Empty;
                string datetype = String.Empty;
                string coord = String.Empty;
                foreach (var attr in t.Arguments)
                {
                    var attrName = MwParserUtility.NormalizeTemplateArgumentName(attr.Name);
                    if (attrName == null)
                    {
                    }
                    else if (attrName.Contains("coord"))
                    {
                        coord = attr.Value.ToString().Trim();
                    }
                    else if (attrName.Contains("date") && !skipDateTypes.Contains(attrName))
                    {
                        date = attr.Value.ToPlainText().Trim();
                        datetype = attrName;
                    }
                }

                if (date != string.Empty && coord != string.Empty)
                {
                    if (dateTypes.ContainsKey(datetype))
                    {
                        Console.WriteLine("{0}{1}: {2}", title, dateTypes[datetype], date);
                    }
                    else
                    {
                        Console.WriteLine("Unknown datetype: {0}", datetype);
                    }
                    // Console.WriteLine("Title: {0}", title);
                    // Console.WriteLine(MwParserUtility.NormalizeTemplateArgumentName(t.Name));
                    // Console.WriteLine("Date ({0}): {1}", datetype, date);
                    // Console.WriteLine("Coord: {0}", coord);
                }
            }
        }
    }
}
