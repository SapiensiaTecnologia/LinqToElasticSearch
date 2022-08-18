using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoFixture;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses
{
    [Serializable]
    public class TestElasticGroup
    {
        [StringEnum]
        [Keyword]
        public string Name { get; set; }
    }
    
    public class GroupByClauseTest : IntegrationTestsBase<SampleData>
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