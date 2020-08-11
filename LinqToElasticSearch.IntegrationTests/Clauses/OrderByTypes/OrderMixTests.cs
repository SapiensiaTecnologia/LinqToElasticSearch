using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderMixTests: IntegrationTestsBase<SampleData>
    {
        
        [Fact]
        public void OrderDescNumberWithWhere()
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
            //When
            var results = Sut.Where(x => x.Age >= 4).OrderBy(x => x.Can).ThenBy(x => x.Age).ToList();

            //Then
            results.Count().Should().Be(8);
            results[0].Age.Should().Be(4);
            results[0].Can.Should().Be(false);
            results[1].Age.Should().Be(5);
            results[1].Can.Should().Be(true);
        }
        
        
        [Fact]
        public void OrderTwice()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();
            var count = 0;
            var firstGuid = new Guid("3BCBF03A-3BCD-444E-9C66-3F8CA9CC2AEC");
            var secondGuid = new Guid("4A49445C-3A9E-4C0C-8B63-99DEB1A299BB");
            foreach (var data in datas)
            {
                data.Age = count++;
                data.Can = false;
            }

            datas[0].Can = true;
            datas[1].Age = 1;
            datas[1].Can = true;
            datas[1].Id = secondGuid;
            datas[2].Age = 1;
            datas[2].Can = true;
            datas[2].Id = firstGuid;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Can).OrderBy(x => x.Age).ThenBy(x => x.Id).ToList();

            //Then
            results.Count().Should().Be(3);
            results[0].Age.Should().Be(0);
            results[0].Can.Should().Be(true);
            results[1].Age.Should().Be(1);
            results[1].Can.Should().Be(true);
            results[1].Id.Should().Be(firstGuid);
            results[2].Age.Should().Be(1);
            results[2].Can.Should().Be(true);
            results[2].Id.Should().Be(secondGuid);
        }
    }
}