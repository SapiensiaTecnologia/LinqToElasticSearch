using System.Linq;
using FluentAssertions;
using Remotion.Linq.Parsing.Structure;
using Xunit;

namespace LinqToElk.Tests
{
    public class RelinqSampleTests
    {
        private SampleQueryable<SampleDataSourceItem> items;

        public RelinqSampleTests()
        {
            var queryParser = QueryParser.CreateDefault();

            // Create our IQueryable instance
            items = new SampleQueryable<SampleDataSourceItem>(queryParser, new SampleQueryExecutor());
        }

        [Fact]
        public void Test()
        {
            var results = from i in items select i;

            // force evalution of the statement to prevent assertion from re-evaluating the query.
            var list = results.ToList();

            list.Count.Should().Be(10);
            list[3].Name.Should().Be("Name 3");
        }
    }
}
