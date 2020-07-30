using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses
{
    public class WhereClauseTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name == datas[1].Name);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
        
        [Fact]
        public void WhereNotEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name != datas[1].Name);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Name.Should().Be(datas[0].Name);
            listResults[1].Name.Should().Be(datas[2].Name);
        }
        
        [Fact]
        public void WhereContains()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[1].Name = "123456789";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name.Contains("4567"));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
    }
}