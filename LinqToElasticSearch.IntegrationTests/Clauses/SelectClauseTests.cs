using AutoFixture;
using Xunit;
using System.Linq;
using FluentAssertions;

namespace LinqToElasticSearch.IntegrationTests.Clauses
{
    public class SelectClauseTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void SelectOneObject()
        {
            //Given
            var data = Fixture.Create<SampleData>();

            Index(data);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = from i in Sut select i;
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(data.Name);
        }

        [Fact] public void SelectObjects()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(15);

            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut;
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(15);
        }
        
        
        [Fact] public void SelectObjects2()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(15);

            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Select(x => x.Id);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(15);
        }
    }
}