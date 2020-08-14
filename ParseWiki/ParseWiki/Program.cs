using System;
using System.Threading.Tasks;
using ParseWiki.DataTypes;
using ParseWiki.Extractors;
using ParseWiki.Pipelines;
using ParseWiki.Processors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

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
            var connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var datasource = new MySqlDataSource(connstr);
            var source = new XmlWikiSource(filepath, datasource.GetTitleToIdTranslator());
            await foreach (var item in source.FetchAll())
            {
                Console.WriteLine(item);
            }
        }

        static async Task Main3(string[] args)
        {
            var connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var datasource = new MySqlDataSource(connstr);
            const string filepath = "/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml";
            var source = new MediawikiSource(filepath);
            var extractor = new TitleExtractor();
            var sink = datasource.GetTitleSink();
            var proc = new DataflowProcessor<WikiBlock, string>(source, extractor, sink);
            // var proc = new SynchronousProcessor<WikiBlock, string>(source, extractor, sink);
            await proc.Process();
        }

        static async Task Main(string[] args)
        {
            const string connstr = "server=localhost; database=hotw; uid=hotw; pwd=hotw;";
            var datasource = new MySqlDataSource(connstr);
            const string filepath = "/mnt/data/wiki/articles_in_xml_with_id.xml";
            var titleToId = datasource.GetTitleToIdTranslator();
            var wikisource = new XmlWikiSource(filepath, titleToId);
            // var sink = new NullSink<WikiEvent>();
            var sink = datasource.GetWikiEventSink();
            // datasource.TruncateWikiEvents();
            var pipeline = new NlpEventPipeline(
                wikisource,
                sink,
                datasource.GetIdToLocationTranslator(),
                datasource.GetTitleToLocationTranslator(),
                titleToId
            );
            var proc = pipeline.Build();
            
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                proc.Cancel();
            };
            await proc.Process();
        }
    }
}