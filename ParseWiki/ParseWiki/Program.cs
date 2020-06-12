using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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

        static async Task Main(string[] args)
        {
            const string filepath = "/mnt/data/wiki/articles_in_xml.xml";
            var source = new XmlWikiSource(filepath);
            await foreach (var item in source.ReadWikiBlock())
            {
                Console.WriteLine(item);
            }
        }
    }
}