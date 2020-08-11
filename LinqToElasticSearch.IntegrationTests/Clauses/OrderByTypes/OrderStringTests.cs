using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderStringTests: IntegrationTestsBase<SampleData>
    {
        public OrderStringTests()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Name = "abc";
            }

            datas[7].Name = "aab";
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
        }
        
        
        [Fact]
        public void OrderAscNumber()
        {
            //When
            var results = Sut.OrderBy(x => x.Name).ToList();

            //Then
            results.Count().Should().Be(11);
            results.First().Name.Should().Be("aab");
        }
        
        [Fact]
        public void OrderDescNumber()
        {
            //When
            var results = Sut.OrderByDescending(x => x.Name).ToList();

            //Then
            results.Count().Should().Be(11);
            results[10].Name.Should().Be("aab");
        }
    }
}