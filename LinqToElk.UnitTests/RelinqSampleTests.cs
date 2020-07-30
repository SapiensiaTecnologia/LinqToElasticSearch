using System;
using System.Linq;
using FluentAssertions;
using Nest;
using Xunit;

namespace LinqToElk.Tests
{
    public class RelinqSampleTests
    {
        private ElasticQueryable<SampleDataSourceItem> _sut;
        private ElasticClient _elasticClient;
        private const string DataId = "123456789";

        public RelinqSampleTests()
        {
            _elasticClient = ElasticClientFactory.CreateElasticClient("http://localhost:9200", "", "");
            _sut = new ElasticQueryable<SampleDataSourceItem>(_elasticClient, DataId);
        }

        [Fact]
        public void SelectQueryExpression()
        {
            var results = from i in _sut select i;

            // force evalution of the statement to prevent assertion from re-evaluating the query.
            var list = results.ToList();

            list.Count.Should().Be(10);
            list[3].Name.Should().Be("Name 3");
        }
        
        [Fact]
        public void SelectMethodChain()
        {
            var results = _sut.Select(x => x);

            // force evalution of the statement to prevent assertion from re-evaluating the query.
            var list = results.ToList();

            list.Count.Should().Be(10);
            list[3].Name.Should().Be("Name 3");
        }
        
        [Fact]
        public void WhereMethodChain()
        {
            var results = _sut.Select(x => x).Where(x => x.Name == "Name 3");

            // force evalution of the statement to prevent assertion from re-evaluating the query.
            var list = results.ToList();

            list.Count.Should().Be(1);
            list[0].Name.Should().Be("Name 3");
        }

        protected void FlushElasticSearch()
        {
        }
    }
}
