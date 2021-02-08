using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ParseWiki.DataTypes;

namespace ParseWiki.Translators
{
    internal struct Section
    {
        internal int StartIndex { get; }
        internal int EndIndex { get; }
        internal int Level { get; }

        internal Section(int start, int end, int level)
        {
            StartIndex = start;
            EndIndex = end;
            Level = level;
        }
    }
    public class WikitextTranslator : ITranslator<WikiText, XmlDocument>
    {
        public async Task<XmlDocument> Translate(WikiText wikitext)
        {
            // Receives a mediawiki-formatted string that starts with <page> and
            // ends with </page>
            // The string is the XML for a page from a wikipedia dump
            var doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
            var page = doc.CreateNode(string.Empty, "page", string.Empty);
            doc.AppendChild(page);
            var title = doc.CreateNode(string.Empty, "title", string.Empty);
            title.InnerText = wikitext.Title;
            var id = doc.CreateNode(string.Empty, "id", string.Empty);
            id.InnerText = wikitext.Id.ToString();
            if (
                wikitext.NamespaceId != 0 
                || wikitext.Text.Contains(
                    "#redirect", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                return null;
            }

            // var text = new StringBuilder(wikitext.Text);
            var text = HandleCrlf(wikitext.Text);
            var sections = new List<Section>();
            for (var level = 2; level < 7; ++level)
            {
                var equals = new string('=', level);
                var leftMarker = "\0x0A" + equals;
                var rightMarker = equals + "\0x0A";
                var pos2 = 0;
                while (true)
                {
                    var pos1 = text.IndexOf(leftMarker, pos2, StringComparison.Ordinal);
                    if (pos1 == -1)
                        break;
                    pos2 = text.IndexOf(rightMarker, pos1+1, StringComparison.Ordinal);
                    var crPos = text.IndexOf("\x0A", pos1+1, StringComparison.Ordinal);
                    if (pos2 == -1 || crPos < pos2) // no corresopnding right marker found
                    {
                        pos2 = pos1 + 1;
                        continue;
                    }

                    sections.Add(new Section(pos1, pos2, level));
                }
            }

            var numSections = sections.Count;
            var breaks = new List<int>(2 * numSections + 1) {0};
            foreach (var section in sections)
            {
                breaks.Add(section.StartIndex);
                breaks.Add(section.EndIndex);
            }
            breaks.Add(text.Length);
            var textNode = doc.CreateNode(string.Empty, "text", string.Empty);
            page.AppendChild(textNode);
            for (var i = 0; i < sections.Count + 1; ++i)
            {
                if (i == 0)
                {
                    var secLength = breaks[1] - breaks[0];
                    if (secLength > 0)
                    {
                        var curString = "<firstPara>" + text.Substring(breaks[0], breaks[1] - breaks[0]) +
                                        "</firstPara>";
                        var node = ParseSection(curString);
                        textNode.AppendChild(node);
                    }
                }
                else
                {
                    var section = doc.CreateElement("section");
                    section.SetAttribute("level", sections[i].Level.ToString());
                    textNode.AppendChild(section);
                }
            }
            return doc;
        }

        private static XmlNode ParseSection(string sectionText)
        {
            return null;
        }

        private static string HandleCrlf(string text)
        {
            // replace CRLF with LF, and then any remaining CR with LF
            return text.Replace(
                "\x0D\x0A",
                "\x0A"
            ).Replace(
                '\x0D',
                '\x0A'
            );
        }
    }
}