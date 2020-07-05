using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using NUnit.Framework;
using ParseWiki;
using ParseWiki.Sources;

namespace ParseWikiTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestDateRangeParse()
        {
            var range = DateRange.Parse("22 January – 5 June 1944 (136 days)");
            Assert.AreEqual(range.StartTime, new PWDateTime(1944, 1, 22, 0, 0, 0));
            Assert.AreEqual(range.EndTime, new PWDateTime(1944, 6, 5, 23, 59, 59));
            range = DateRange.Parse("2580–2560 BC (4th dynasty)");
            Assert.AreEqual(
                new PWDateTime(2580, 1, 1, 0, 0, 0, Epoch.BC),
                range.StartTime
            );
            Assert.AreEqual(
                new PWDateTime(2560, 12, 31, 23, 59, 59, Epoch.BC),
                range.EndTime
            );
        }

        [Test]
        public void TestCoordParse()
        {
            var coord = Coord.FromWikitext("{{coord|43.651234|-79.383333}}");
            Assert.AreEqual(coord.Latitude,43.651234, 1e-5);
            Assert.AreEqual(coord.Longitude, -79.383333, 1e-5);
            coord = Coord.FromWikitext("{{coord|43.653500|N|79.384000|W}} ");
            Assert.AreEqual(coord.Latitude,43.653500, 1e-5);
            Assert.AreEqual(coord.Longitude, -79.384000, 1e-5);
            coord = Coord.FromWikitext("{{coord|43|29|N|79|23|W}} ");
            Assert.AreEqual(coord.Latitude,43.483333333333334, 1e-5);
            Assert.AreEqual(coord.Longitude, -79.38333333333334, 1e-5);
            coord = Coord.FromWikitext("{{coord|22|54|30|S|43|14|37|W}} ");
            Assert.AreEqual(coord.Latitude,-22.90833333333333, 1e-5);
            Assert.AreEqual(coord.Longitude, -43.243611111111115, 1e-5);

        }

        [Test]
        public void TestSettlementInfobox()
        {
            var text = File.ReadAllText("settlement_infobox.txt");
            var block = new WikiBlock(0, "San Francisco", text);
            var template = block.Wtext.EnumDescendants().OfType<Template>().First();
            var infobox = Infobox.FromWiki(block, template);
            Console.Out.WriteLine(infobox.Location);
            Assert.AreEqual(infobox.Location.Latitude, 37.7775, 1e-5);
            Assert.AreEqual(infobox.Location.Longitude, -122.416388, 1e-5);
        }

        [Test]
        public void TestParseProblemWiki1()
        {
            var text = File.ReadAllText("problem_wiki1.txt");
            var parser = new WikitextParser();
            var wtext = parser.Parse(text);
        }

        [Test]
        public void TestAwsConnection()
        {
            var source = new DynamoDbSource();
        }
    }
}