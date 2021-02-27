using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ParseWiki.DataTypes;

namespace ParseWiki.Sources
{
    public class PageXmlSource : ISource<PageXml>
    {
        private readonly string _inputPath;

        public PageXmlSource(string inputPath)
        {
            _inputPath = inputPath;
        }
        
        public async IAsyncEnumerable<PageXml> FetchAll()
        {
            using var inputReader = File.OpenText(_inputPath);
            string line;
            string title = null;
            long? id = null;
            StringBuilder contents = null;
            var titleMatcher = new Regex("<title>(?<title>.*?)</title>");
            var idMatcher = new Regex("<id>(?<id>.*?)</id>");
            while ((line = await inputReader.ReadLineAsync()) != null)
            {
                if (line.Contains("<page>"))
                {
                    contents = new StringBuilder();
                }
                if (contents == null)
                    continue;
                contents.AppendLine(line);
                if (title == null)
                {
                    var titleMatch = titleMatcher.Matches(line).FirstOrDefault();
                    if (titleMatch != null)
                    {
                        title = await WikiUtil.ReadNodeText(titleMatch.Value);
                    }
                }

                if (id == null)
                {
                    var match = idMatcher.Matches(line).FirstOrDefault();
                    if (match != null)
                    {
                        id = long.Parse(await WikiUtil.ReadNodeText(match.Value));
                    }
                }
                if (line.Contains("</page>"))
                {
                    if (!id.HasValue)
                        throw new ApplicationException("No id found");
                    if (title == null)
                        throw new ApplicationException("No title found");
                    yield return new PageXml(id.Value, title, contents.ToString());
                    contents = null;
                    id = null;
                    title = null;
                }
            }
        }
    }
}