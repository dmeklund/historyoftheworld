using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParseWiki.Extractors;
using ParseWiki.Processors;

namespace ParseWiki
{
    class Program
    {
        static async Task Main2(string[] args)
        {
            const string filepath = "/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml";
            var connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var datasource = new MySqlDataSource(connstr);
            // datasource.TruncateEvents();
            datasource.TruncateLocations();
            var parser = new WikiParser(filepath, datasource);
            await parser.Parse();
        }

        static async Task Main1(string[] args)
        {
            const string filepath = "/mnt/data/wiki/articles_in_xml.xml";
            var source = new XmlWikiSource(filepath);
            await foreach (var item in source.FetchAll())
            {
                Console.WriteLine(item);
            }
        }

        static async Task Main(string[] args)
        {
            var connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var datasource = new MySqlDataSource(connstr);
            const string filepath = "/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml";
            var source = new MediawikiSource(filepath);
            var extractor = new TitleExtractor();
            var sink = datasource.GetTitleSink();
            var proc = new DataflowProcessor<WikiBlock, string>(source, extractor, sink);
            await proc.Process();
        }
    }
}