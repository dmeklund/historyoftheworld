using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace XmlPrettyPrint
{
    class Program
    {
        static void SuperSimple(string inputPath, string outputPath)
        {
            using var instream = File.OpenText(inputPath);
            using var outstream = new StreamWriter(outputPath);
            var buffer = new char[1024];
            int offset = 0;
            int count = 1024;
            int numread;
            while ((numread = instream.Read(buffer, offset, count)) > 0)
            {
                var astext = new string(buffer, 0, numread);
                var splitind = astext.IndexOf("<page", StringComparison.Ordinal);
                if (splitind > 0)
                {
                    outstream.Write(astext.Substring(0, splitind));
                    outstream.Write("\n");
                    outstream.Write(astext.Substring(splitind, numread - splitind));
                }
                else
                {
                    outstream.Write(astext);
                }
            }
        }

        static async Task Main(string[] args)
        {
            const string inputPath = "/mnt/data/wiki/articles_in_xml_indented.xml";
            const string outputPath = "/mnt/data/wiki/articles_in_xml_indented2.xml";
            // Reformat(inputPath, outputPath);
            // await ProcessLines(inputPath, outputPath);
            await using var stream = File.OpenRead(outputPath);
            await CheckXml(stream);
        }

        private static async Task ProcessLines(string inputPath, string outputPath)
        {
            using var inputReader = File.OpenText(inputPath);
            await using var outputWriter = File.AppendText(outputPath);
            string line;
            var declaration = "";
            // int count = 0;
            var pageMatcher = new Regex("<page.*?</page>");
            var skip = true;
            while ((line = await inputReader.ReadLineAsync()) != null)
            {
                if (skip)
                {
                    var title = ExtractTitle(line);
                    if (title == "Emma Seehofer")
                    {
                        skip = false;
                    }
                    continue;
                }
                if (line.StartsWith("<?xml"))
                {
                    declaration = line;
                    await outputWriter.WriteLineAsync(line);
                }
                else if (line.StartsWith("<pages>"))
                {
                    var length = "<pages>".Length;
                    await outputWriter.WriteLineAsync("<pages>");
                    line = line.Substring(length, line.Length - length);
                    if (await IsValidXml(declaration, line))
                    {
                        await outputWriter.WriteLineAsync(line);
                    }
                }
                else if (line.EndsWith("</pages>"))
                {
                    var length = "</pages>".Length;
                    line = line.Substring(0, line.Length - length);
                    if (await IsValidXml(declaration, line))
                    {
                        await outputWriter.WriteLineAsync(line);
                    }
                    await outputWriter.WriteLineAsync("</pages>");
                }
                else if (line.LastIndexOf("<page", StringComparison.Ordinal) != 0)
                {
                    foreach (Match match in pageMatcher.Matches(line))
                    {
                        if (await IsValidXml(declaration, match.Value))
                        {
                            await outputWriter.WriteLineAsync(match.Value);
                        }
                    }
                }
                else if (await IsValidXml(declaration, line))
                {
                    await outputWriter.WriteLineAsync(line);
                }
            }
        }

        private static async Task<bool> IsValidXml(string declaration, string line)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(declaration + line));
            try
            {
                await CheckXml(stream);
            }
            catch (XmlException e)
            {
                Console.WriteLine($"Skipping article '{ExtractTitle(line)}' due to invalid XML ({e.Message})");
                return false;
            }

            return true;
        }

        private static async Task CheckXml(Stream stream)
        {
            using var reader = XmlReader.Create(stream, new XmlReaderSettings() {Async = true});
            while (await reader.ReadAsync())
            {
            }
        }

        private static string ExtractTitle(string line)
        {
            var title = "";
            var offset = line.IndexOf("<title>", StringComparison.Ordinal) + "<title>".Length;
            var length = line.IndexOf("</title>", StringComparison.Ordinal) - offset;
            if (offset >= 0 && length > 0)
            {
                title = line.Substring(offset, length);
            }
            else
            {
                Console.WriteLine($"Couldn't extract title from line: {line}");
            }
            return title;
        }

        static void Reformat(string inputPath, string outputPath)
        {
            var namespaces = new List<string>()
            {
                "h", "f", "xsl", "rdf", "tal", "gml", "abc", "xlink", "xacml3", "esi", "opt", "text", "sx"
            };

            var undeclaredPrefix = new Regex("'(?<prefix>.*?)' is an undeclared prefix");
            var success = false;
            while (true)
            {
                try
                {
                    Console.WriteLine(@"Attempting with prefix list: ""{0}""", string.Join(@""", """, namespaces));
                    AttemptParseXml(inputPath, outputPath, namespaces);
                    success = true;
                }
                catch (XmlException e)
                {
                    var match = undeclaredPrefix.Match(e.Message);
                    if (match.Success)
                    {
                        var newPrefix = match.Groups["prefix"].Value;
                        Console.WriteLine($"Retrying with newly found prefix: {newPrefix}");
                        namespaces.Add(newPrefix);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (success)
                {
                    break;
                }
            }

            Console.WriteLine("Final prefix list: {0}", string.Join(" ", namespaces));
        }

        private static void AttemptParseXml(string inputPath, string outputPath, IEnumerable<string> namespaces)
        {
            using var stream = File.OpenRead(inputPath);
            var readerSettings = new XmlReaderSettings {NameTable = new NameTable()};
            var xmlns = new XmlNamespaceManager(readerSettings.NameTable);
            foreach (var ns in namespaces)
            {
                xmlns.AddNamespace(ns, "http://example.com");
            }

            var context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            using var reader = XmlReader.Create(stream, readerSettings, context);
            var writerSettings = new XmlWriterSettings {Indent = true};
            using var writer = XmlWriter.Create(outputPath, writerSettings);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.None:
                        break;
                    case XmlNodeType.Element:
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        writer.WriteAttributes(reader, false);
                        if (reader.IsEmptyElement)
                        {
                            writer.WriteEndElement();
                        }

                        break;
                    case XmlNodeType.Attribute:
                        break;
                    case XmlNodeType.Text:
                        writer.WriteString(reader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        writer.WriteCData(reader.Value);
                        break;
                    case XmlNodeType.EntityReference:
                        writer.WriteEntityRef(reader.Name);
                        break;
                    case XmlNodeType.Entity:
                        writer.WriteString(reader.Value);
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
                        var whitespace = reader.Value.Replace("\n", "");
                        if (whitespace.Length > 0)
                        {
                            writer.WriteString(whitespace);
                        }

                        break;
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    case XmlNodeType.EndElement:
                        writer.WriteEndElement();
                        break;
                    case XmlNodeType.EndEntity:
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}