using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace ParseWiki
{
    public class MediawikiSource : IWikiSource
    {
        private string _filepath;
        
        public MediawikiSource(string filePath)
        {
            _filepath = filePath;
        }
        
        public async IAsyncEnumerable<WikiBlock> ReadWikiBlock()
        {
            await using var stream = File.OpenRead(_filepath);
            var settings = new XmlReaderSettings() {Async = true};
            using var reader = XmlReader.Create(stream, settings);
            var parentElements = new Stack<string>();
            string title = "";
            int id = 0;
            var counter = 0;
            var isTitle = false;
            var isId = false;
            var isText = false;
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
                            // if (title != "Kid Chocolate")
                            // {
                            // await extractor.SendAsync(new WikiBlock(id, title, text));
                            if (text.Contains("coord", StringComparison.InvariantCultureIgnoreCase))
                            {
                                yield return new WikiBlock(id, title, text);
                                // ThreadPool.QueueUserWorkItem(ExtractLocations, new WikiBlock(id, title, text), true);
                            }
                            // var result = ExtractLocations(new WikiBlock(id, title, text));
                            // await SaveLocation(result);
                            // }
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
        }
    }
}