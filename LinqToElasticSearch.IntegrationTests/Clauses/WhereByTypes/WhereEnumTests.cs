using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereEnumTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereEnumEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.SampleTypeProperty = SampleType.SampleType;
            }

            datas[1].SampleTypeProperty = SampleType.Type;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.SampleTypeProperty == SampleType.Type);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].SampleTypeProperty.Should().Be(datas[1].SampleTypeProperty);
        }
        
        [Fact]
        public void WhereStringEnumEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.SampleTypePropertyString = SampleType.SampleType;
            }

            datas[1].SampleTypePropertyString = SampleType.Type;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.SampleTypePropertyString == SampleType.Type);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].SampleTypePropertyString.Should().Be(datas[1].SampleTypePropertyString);
        }
        
        [Fact]
        public void WhereStringEnumNotEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.SampleTypePropertyString = SampleType.SampleType;
            }

            datas[1].SampleTypePropertyString = SampleType.Type;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.SampleTypePropertyString != SampleType.Type);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
        }
    }
}