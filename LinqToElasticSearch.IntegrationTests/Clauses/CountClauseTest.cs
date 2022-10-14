using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses
{
    public class CountClauseTest: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void CountObjects()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11);

            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Count();

            //Then
            results.Should().Be(11);
        }
        
        [Fact]
        public void CountWhereStringEnumObjects()
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
            var results = Sut.Count(x => x.SampleTypePropertyString == SampleType.Type);

            //Then
            results.Should().Be(1);
        }

        [Fact]
        public void CountWithMoreThan10000Objects()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(10006);

            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Count();

            //Then
            results.Should().Be(10000);
        }
    }
}