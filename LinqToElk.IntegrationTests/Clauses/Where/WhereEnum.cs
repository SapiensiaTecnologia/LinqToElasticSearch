using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.Where
{
    public class WhereEnum: IntegrationTestsBase<SampleData>
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
    }
}