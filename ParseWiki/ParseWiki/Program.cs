using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParseWiki
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string filepath = "/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml";
            var parser = new WikiParser(filepath);
            await parser.Parse();
        }
    }
}