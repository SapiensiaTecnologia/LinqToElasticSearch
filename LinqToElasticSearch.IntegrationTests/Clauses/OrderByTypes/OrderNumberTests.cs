using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderClauseTests: IntegrationTestsBase<SampleData>
    {
        public OrderClauseTests()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Age = 30;
                data.Can = true;
            }
            datas[7].Age = 23;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
        }
        
        [Fact]
        public void OrderAscNumber()
        {
            //When
            var results = Sut.OrderBy(x => x.Age).ToList();

            //Then
            results.Count().Should().Be(11);
            results.First().Age.Should().Be(23);
        }
        
        [Fact]
        public void OrderDescNumber()
        {
            //When
            var results = Sut.OrderByDescending(x => x.Age).ToList();

            //Then
            results.Count().Should().Be(11);
            results[10].Age.Should().Be(23);
        }
    }
}