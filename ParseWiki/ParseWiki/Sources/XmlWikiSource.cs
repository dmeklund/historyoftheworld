using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ParseWiki.DataTypes;
using ParseWiki.Translators;

namespace ParseWiki.Sources
{
    public class XmlWikiSource : ISource<WikiPageLazyLoadId>
    {
        private string _filepath;
        private readonly ITranslator<string, int?> _titleToId;
        public XmlWikiSource(string wikiXmlPath, ITranslator<string, int?> titleToId)
        {
            _filepath = wikiXmlPath;
            _titleToId = titleToId;
        }
        
        public async IAsyncEnumerable<WikiPageLazyLoadId> FetchAll()
        {
            await using var stream = File.OpenRead(_filepath);
            var settings = new XmlReaderSettings() {Async = true};
            using var reader = XmlReader.Create(stream, settings);
            var parentElements = new Stack<string>();
            string title = "";
            var isText = false;
            StringBuilder pageText = null;
            string linkTarget = null, linkAnchor = null;
            var links = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            var skipArticles = new Random().Next(0, 1000);
            var count = 0;
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
                        switch (reader.Name)
                        {
                            case "text":
                                isText = true;
                                pageText = new StringBuilder();
                                break;
                            case "par":
                                pageText.Append("\n");
                                break;
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
                        else if (parentElementName == "target")
                        {
                            linkTarget = reader.Value;
                        }
                        else if (parentElementName == "anchor")
                        {
                            linkAnchor = reader.Value;
                            // anchor value also gets included in the page text
                            pageText.Append(reader.Value);
                            pageText.Append(" ");
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
                            var result = new WikiPageLazyLoadId(title, text, _titleToId)
                            {
                                Links = links
                            };
                            yield return result;
                            links = new Dictionary<string, int>();
                        }
                        else if (reader.Name == "link")
                        {
                            linkAnchor ??= linkTarget;
                            linkTarget ??= linkAnchor;
                            if (linkAnchor == null || linkTarget == null)
                            {
                                Console.WriteLine($"Invalid link found in {title}: {linkAnchor} -> {linkTarget}");
                            }
                            if (count > ++skipArticles)
                            {
                                await ProcessLink(linkAnchor, linkTarget, links);
                            }
                            linkAnchor = null;
                            linkTarget = null;
                        }
                        parentElements.Pop();
                        break;
                }
            }

        }

        private async Task ProcessLink(string linkAnchor, string linkTarget, Dictionary<string, int> links)
        {
            if (linkAnchor == null || linkTarget == null)
            {
                // throw new ApplicationException("Link missing target or anchor");
                // Console.WriteLine("Link missing target or anchor");
                return;
            }

            var id = await _titleToId.Translate(linkTarget);
            if (id == null)
            {
                // Console.WriteLine($"Warning: target not found: {linkTarget}");
            }
            else
            {
                links[linkTarget] = id.Value;
                if (linkTarget != linkAnchor)
                {
                    links[linkAnchor] = id.Value;
                }
            }
        }
    }
}