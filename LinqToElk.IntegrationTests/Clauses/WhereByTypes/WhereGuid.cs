using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereGuid: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereGuidEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.Id = Guid.NewGuid();
            }

            datas[1].Id = Guid.NewGuid();
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Id == datas[1].Id);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Id.Should().Be(datas[1].Id);
        }
    }
}