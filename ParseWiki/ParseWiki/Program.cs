using System;

namespace ParseWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepath;
            filepath = "/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml";
            var parser = new WikiParser(filepath);
            parser.Parse();
        }
    }
}