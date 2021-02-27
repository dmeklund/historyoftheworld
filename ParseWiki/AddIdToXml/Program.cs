using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ParseWiki.Sources;
using ParseWiki.Translators;

namespace AddIdToXml
{
    class Program
    {
        private static IDictionary<string, long> _titleToId;
        static async Task Main(string[] args)
        {
            const string connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var source = new MySqlDataSource(connstr);
            _titleToId = await source.GetAllTitleToIds();
            const string inputPath = "/mnt/data/wiki/articles_in_xml_indented.xml";
            const string outputPath = "/mnt/data/wiki/articles_in_xml_indented2.xml";
            await AddIdToXml(inputPath, outputPath);
        }

        static async Task AddIdToXml(string inputPath, string outputPath)
        {
            using var inputReader = File.OpenText(inputPath);
            await using var outputWriter = File.CreateText(outputPath);
            string line;
            var titleMatcher = new Regex("<title>(?<title>.*?)</title>");
            var batchSize = 1000;
            var taskList = new List<Task>();
            var declaration = "";
            while ((line = await inputReader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("<?xml"))
                {
                    declaration = line;
                    await outputWriter.WriteLineAsync(line);
                }
                else if (line.StartsWith("</pages>") || line.StartsWith("<pages>"))
                {
                    await outputWriter.WriteLineAsync(line);
                }
                else
                {
                    var match = titleMatcher.Matches(line).FirstOrDefault();
                    if (match == null)
                    {
                        Console.WriteLine($"No title found: {line}");
                        continue;
                    }

                    var titleXml = match.Value;
                    var title = await ExtractTitle(declaration, titleXml);
                    if (_titleToId.TryGetValue(title, out var id))
                    {
                        var newLine = titleMatcher.Replace(line, $"{titleXml}<id>{id}</id>");
                        await outputWriter.WriteLineAsync(newLine);
                    }
                    else
                    {
                        Console.WriteLine($"Couldn't find ID for title: {title}");
                    }
                }

                if (taskList.Count >= batchSize)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
        }

        private static async Task<string> ExtractTitle(string declaration, string titleXml)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(declaration + titleXml));
            using var reader = XmlReader.Create(stream, new XmlReaderSettings() {Async = true});
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    return reader.Value.Replace("&quot;", "\"");
                }
            }

            return null;
        }
    }
}