using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.Where
{
    public class WhereStringTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereStringEqual()
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
        public void WhereStringNotEqual()
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
        public void WhereStringContains()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Name = "abd";
            datas[1].Name = "abcdefgh";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name.Contains("abc"));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
        
        [Fact]
        public void WhereStringContainsToLower()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[1].Name = "abcdefgh";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name.Contains("DefG".ToLower()));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
        
        [Fact]
        public void WhereStringNotContains()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Name = "xxxxxxxx";
            datas[1].Name = "abcdefgh";
            datas[2].Name = "yyyyyyyy";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => !x.Name.Contains("defg"));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Name.Should().Be(datas[0].Name);
            listResults[1].Name.Should().Be(datas[2].Name);
        }
        
        [Fact]
        public void WhereStringStartWith()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Name = "0123456789";
            datas[1].Name = "123456789";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name.StartsWith("1234"));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
        
        [Fact]
        public void WhereStringEndWith()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[0].Name = "123456789";
            datas[1].Name = "12345678";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Name.EndsWith("5678"));
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        }
    }
}