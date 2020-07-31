using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.Where
{
    public class WhereMix: IntegrationTestsBase<SampleData>
    {
       
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhereTwice(bool together)
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();

            datas[1].Name = "123456789";
            datas[1].Age = 23;
            datas[2].Name = "123456789";
            
            Bulk(datas);

            ElasticClient.Indices.Refresh();
            
            //When
            IQueryable<SampleData> results;
            if (together)
            {
                results = Sut.Where(x => x.Name.Contains("4567") && x.Age == 23);
            }
            else
            {
                results = Sut.Where(x => x.Name.Contains("4567"));
                results = results.Where(x => x.Age == 23);
            }
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be(datas[1].Name);
        } 
    }
}