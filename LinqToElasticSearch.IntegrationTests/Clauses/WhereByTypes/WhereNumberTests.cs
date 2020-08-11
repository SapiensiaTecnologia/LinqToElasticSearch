using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereNumberTests: IntegrationTestsBase<SampleData>
    {
        
        [Fact]
        public void WhereNumberEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age == datas[1].Age);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
        
        [Fact]
        public void WhereNumberNotEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age != datas[1].Age);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Name.Should().Be(datas[0].Name);
            listResults[1].Name.Should().Be(datas[2].Name);
        }
        
        [Fact]
        public void WhereNumberGreaterThen()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Age = 18;
            datas[1].Age = 23;
            datas[2].Age = 16;
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age > 18).ToList();

            //Then
            results.Count.Should().Be(1);
            results[0].Age.Should().Be(datas[1].Age);
        }
        
        [Fact]
        public void WhereNumberGreaterThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Age = 18;
            datas[1].Age = 23;
            datas[2].Age = 16;
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age >= 18).ToList();

            //Then
            results.Count.Should().Be(2);
            results[0].Age.Should().Be(datas[0].Age);
            results[1].Age.Should().Be(datas[1].Age);
        }
        
        [Fact]
        public void WhereNumberLessThan()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Age = 18;
            datas[1].Age = 23;
            datas[2].Age = 16;
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age < 18).ToList();

            //Then
            results.Count.Should().Be(1);
            results[0].Age.Should().Be(datas[2].Age);
        }
        
        [Fact]
        public void WhereNumberLessThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Age = 18;
            datas[1].Age = 23;
            datas[2].Age = 16;
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Age <= 18).ToList();

            //Then
            results.Count.Should().Be(2);
            results[0].Age.Should().Be(datas[0].Age);
            results[1].Age.Should().Be(datas[2].Age);
        }
    }
}