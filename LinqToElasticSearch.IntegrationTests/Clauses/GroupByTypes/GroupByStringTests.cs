using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.GroupByTypes
{
    
    public class GroupByStringTests : IntegrationTestsBase<SampleData>
    {
        
        [Fact]
        public void GroupByStringEqualWithUniqueGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            datas[0].Name = "abcdef";
            datas[1].Name = "abcdef";
            datas[2].Name = "abcdef";
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => x.Name).ToList();
            
            //Then
            results.Count.Should().Be(2);
            results.Should().ContainSingle(x =>
                x.Key == "abcdef"
                && x.Count() == 3);
            results.Should().ContainSingle(x =>
                x.Key == datas[3].Name
                && x.Count() == 1);
        }
        
        [Fact]
        public void GroupByStringEqualWithAnonymousClassGroupingAndSorting()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            datas[0].Name = "abcdef";
            datas[1].Name = "abcdef";
            datas[2].Name = "abcdef";
            datas[3].Name = "bcdefg";
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => new { I0 = x.Name }).OrderBy(x => x.Key.I0 ).ToList();
            
            //Then
            results.Count.Should().Be(2);
            results.Should().ContainSingle(x =>
                x.Key.I0 == "abcdef"
                && x.Count() == 3);
            results.Should().ContainSingle(x =>
                x.Key.I0 == datas[3].Name
                && x.Count() == 1);

            results[0].Key.I0.Should().Be("abcdef");
            results[1].Key.I0.Should().Be("bcdefg");
        }

        
        [Fact]
        public void GroupByStringEqualWithUniquePropertyGroupingAndSorting()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            datas[0].Name = "abcdef";
            datas[1].Name = "abcdef";
            datas[2].Name = "abcdef";
            datas[3].Name = "bcdefg";
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => x.Name).OrderBy(x => x.Key).ToList();
            
            //Then
            results.Count.Should().Be(2);
            results.Should().ContainSingle(x =>
                x.Key == "abcdef"
                && x.Count() == 3);
            results.Should().ContainSingle(x =>
                x.Key == datas[3].Name
                && x.Count() == 1);

            results[0].Key.Should().Be("abcdef");
            results[1].Key.Should().Be("bcdefg");
        }

        [Fact]
        public void GroupByStringEqualWithMultipleGroupingAndSorting()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            datas[0].Name = "abcdef";
            datas[0].Can = true;
            datas[1].Name = "abcdef";
            datas[1].Can = true;
            datas[2].Name = "abcdef";
            datas[2].Can = false;
            datas[3].Name = "bcdefg";
            datas[3].Can = false;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.GroupBy(x => new {x.Name, Pode = x.Can})
                .OrderBy(x => x.Key.Name)
                .ThenBy(x => x.Key.Pode)
                .ToList();
            
            //Then
            results.Count.Should().Be(3);
            results[0].Key.Name.Should().Be("abcdef");
            results[0].Key.Pode.Should().Be(false);
            
            results[1].Key.Name.Should().Be("abcdef");
            results[1].Key.Pode.Should().Be(true);
            
            results[2].Key.Name.Should().Be("bcdefg");
            results[2].Key.Pode.Should().Be(false);
            
            results.Should().ContainSingle(x =>
                x.Key.Name == "abcdef"
                && x.Key.Pode == true
                && x.Count() == 2);
            results.Should().ContainSingle(x =>
                x.Key.Name == "abcdef"
                && x.Key.Pode == false
                && x.Count() == 1);
            results.Should().ContainSingle(x =>
                x.Key.Name == "bcdefg"
                && x.Key.Pode == false
                && x.Count() == 1);
            results.Should().NotContain(x =>
                x.Key.Name == "bcdefg"
                && x.Key.Pode == true);
        }
        
        [Fact]
        public void GroupByStringEqualWithMultipleGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            datas[0].Name = "abcdef";
            datas[0].Can = true;
            datas[1].Name = "abcdef";
            datas[1].Can = true;
            datas[2].Name = "abcdef";
            datas[2].Can = false;
            datas[3].Name = "1abcdef";
            datas[3].Can = false;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.GroupBy(x => new {x.Name, x.Can}).ToList();
            
            //Then
            results.Count.Should().Be(3);
            results.Should().ContainSingle(x =>
                x.Key.Name == "abcdef"
                && x.Key.Can == true
                && x.Count() == 2);
            results.Should().ContainSingle(x =>
                x.Key.Name == "abcdef"
                && x.Key.Can == false
                && x.Count() == 1);
            results.Should().ContainSingle(x =>
                x.Key.Name == "1abcdef"
                && x.Key.Can == false
                && x.Count() == 1);
            results.Should().NotContain(x =>
                x.Key.Name == "1abcdef"
                && x.Key.Can == true);
        }
        
        
    }
}