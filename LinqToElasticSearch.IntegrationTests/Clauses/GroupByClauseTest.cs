using System.Linq;
using AutoFixture;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses
{
    public class TestElasticGroup
    {
        [StringEnum]
        [Keyword]
        public SampleType SampleTypeProperty { get; set; }
        
        public string Name { get; set; }
        public string Teste { get; set; }
    }
    
    public class GroupByClauseTest : IntegrationTestsBase<SampleData>
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
        
        [Fact]
        public void GroupByPropertySampleType()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.SampleTypeProperty = SampleType.Sample;
            }

            datas[1].SampleTypeProperty = SampleType.Type;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => x.SampleTypeProperty);

            //Then
            results.Should().HaveCount(2);
        }
    }
}