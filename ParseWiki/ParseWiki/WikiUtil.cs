using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public static class WikiUtil
    {
        public static string NormalizeTemplateName(string argumentName)
        {
            // MwParserUtility.NormalizeTemplateArgumentName does not handle HTML comments
            return Regex.Replace(argumentName, "<!--.*?-->", "").Trim();
        }

        public static string NormalizeTemplateName(Node argumentName)
        {
            return NormalizeTemplateName(argumentName.ToString());
        }

        public static async Task<string> ReadNodeText(string rawXml)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawXml));
            using var reader = XmlReader.Create(stream, new XmlReaderSettings() {Async = true});
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    return reader.Value;
                }
            }

            return null;
        }
    }
}