using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class GroupByStringTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void GroupByStringEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            
            datas[0].Name = "abcdef";
            datas[1].Name = "abcdef";
            datas[2].Name = "abcdef";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => x.Name);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Key.Should().Be(datas[1].Name);
        }
    }
}