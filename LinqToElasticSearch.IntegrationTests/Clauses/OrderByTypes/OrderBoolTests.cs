using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderBoolTests: IntegrationTestsBase<SampleData>
    {
        
        public OrderBoolTests()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();
            var count = 0;
            foreach (var data in datas)
            {
                data.Can = true;
                data.Age = count++;
            }

            datas[7].Can = false;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
        }
        
        
        [Fact]
        public void OrderAscNumber()
        {
            //When
            var results = Sut.OrderBy(x => x.Can).ToList();

            //Then
            results.Count().Should().Be(11);
            results.First().Can.Should().Be(false);
        }
        
        [Fact]
        public void OrderDescNumber()
        {
            //When
            var results = Sut.OrderByDescending(x => x.Can).ToList();

            //Then
            results.Count().Should().Be(11);
            results[10].Can.Should().Be(false);
        }
        
        
        [Fact]
        public void OrderDescNumberWithWhere()
        {
            //When
            var results = Sut.Where(x => x.Age > 5).OrderBy(x => x.Can).ToList();

            //Then
            results.Count().Should().Be(5);
            results[0].Age.Should().Be(7);
            results[0].Can.Should().Be(false);
        }
        
        
        [Fact]
        public void OrderDescNumberWithWhereFirstOrDefault()
        {
            //When
            // var results = Sut.Where(x => x.Age > 5).OrderBy(x => x.Can).ToList();
            var single = Sut.Where(x => x.Age > 5).OrderBy(x => x.Can).FirstOrDefault();
            
            //Then
            single.Should().NotBeNull();
            single.Age.Should().Be(7);
            single.Can.Should().Be(false);
        }
        
        [Fact]
        public void OrderDescNumberWithWhereSingle()
        {
            //When
            var single = Sut.Where(x => x.Age > 5).OrderBy(x => x.Can).FirstOrDefault();
            single.Should().NotBeNull();
            
            single = Sut.OrderBy(x => x.Can).SingleOrDefault();
            
            //Then
            single.Should().NotBeNull();
            single.Age.Should().Be(7);
            single.Can.Should().Be(false);
        }
    }

}