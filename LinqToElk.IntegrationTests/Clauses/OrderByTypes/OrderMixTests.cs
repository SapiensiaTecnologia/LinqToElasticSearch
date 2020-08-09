using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderMixTests: IntegrationTestsBase<SampleData>
    {
        public OrderMixTests()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();
            var count = 11;
            foreach (var data in datas)
            {
                data.Can = true;
                data.Age = count--;
            }

            datas[7].Can = false;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
        }
        
        [Fact]
        public void OrderDescNumberWithWhere()
        {
            //When
            var results = Sut.Where(x => x.Age >= 4).OrderBy(x => x.Can).ThenBy(x => x.Age).ToList();

            //Then
            results.Count().Should().Be(8);
            results[0].Age.Should().Be(4);
            results[0].Can.Should().Be(false);
            results[1].Age.Should().Be(5);
            results[1].Can.Should().Be(true);
        }
    }
}