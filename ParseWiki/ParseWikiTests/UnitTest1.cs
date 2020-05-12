using System;
using NUnit.Framework;
using ParseWiki;

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
    }
}