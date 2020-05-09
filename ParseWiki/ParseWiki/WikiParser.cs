using System;
using System.IO;
using System.Xml;

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
                            var text = reader.Value;
                            if (text.Contains("\n| coord") && text.Contains("\n| date "))
                            {
                                Console.WriteLine("Title {0} matches", title);
                            }
                            isText = false;
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
    }
}
