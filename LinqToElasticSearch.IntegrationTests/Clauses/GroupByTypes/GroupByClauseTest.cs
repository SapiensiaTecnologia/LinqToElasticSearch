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
            var results = Sut.GroupBy(x => x.Name);
            var listResults = results.ToList();
            
            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Key.Should().Be(datas[1].Name);
        }

        [Fact]
        public void GroupByStringEqualWithMultipleGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(3).ToList();
            datas[0].Name = "abcdef";
            datas[0].Can = true;
            datas[1].Name = "abcdef";
            datas[1].Can = true;
            datas[2].Name = "abcdef";
            datas[2].Can = false;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.GroupBy(x => new {x.Name, x.Can });
            var listResults = results.ToList();
            
            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Key.Should().Be(datas[1].Name);
        }
        
        
    }
}