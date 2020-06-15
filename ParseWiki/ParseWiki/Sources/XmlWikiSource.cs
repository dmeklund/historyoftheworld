using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ParseWiki.Sources;

namespace ParseWiki.Sources
{
    public class XmlWikiSource : IWikiSource, ISource<WikiBlock>
    {
        private string _filepath;
        public XmlWikiSource(string wikiXmlPath)
        {
            _filepath = wikiXmlPath;
        }
        
        public async IAsyncEnumerable<WikiBlock> FetchAll()
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
            StringBuilder pageText = null;
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.EndElement && parentElements.Count != reader.Depth)
                {
                    throw new ApplicationException("Failed to track depth correctly");
                }
                parentElements.TryPeek(out var parentElementName);

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "text")
                        {
                            isText = true;
                            pageText = new StringBuilder();
                        }
                        else if (reader.Name == "par")
                        {
                            pageText.Append("\n");
                        }
                        if (!reader.IsEmptyElement)
                        {
                            parentElements.Push(reader.Name);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (parentElementName == "title")
                        {
                            title = reader.Value;
                        }
                        else if (isText)
                        {
                            pageText.Append(reader.Value);
                            pageText.Append(" ");
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "text")
                        {
                            var text = pageText.ToString();
                            Console.WriteLine("Finished");
                            yield return new WikiBlock(id, title, text);
                        }
                        parentElements.Pop();
                        break;
                }
            }

        }
    }
}