using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses
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
    }
}