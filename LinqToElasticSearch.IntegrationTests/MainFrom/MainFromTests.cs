using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.MainFrom
{
    public class MainFromTests : IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void MustSearchWithMainFromFormat()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Id = null;
            }
            
            
            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = (from i in Sut 
                where i.Id == null
                orderby i.Name, i.Age
                select i)
                .Skip(5).Take(3);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(3);
        }
        
        [Fact]
        public void MustSearchWithMainFromFormat2()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Name = null;
            }
            
            
            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = (from i in Sut 
                    where i.Name == null
                    orderby i.Date, i.Age
                    select i.Id)
                .Skip(5).Take(3);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(3);
        }
        
        [Fact]
        public void MustSearchWithMainFromFormat3()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Name = null;
            }
            
            
            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = (from i in Sut 
                    where i.Name == null
                    orderby i.Date, i.Age
                    select i)
                .Skip(5).Take(3).Select(x => x.Id);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(3);
        }
        
        
        [Fact]
        public void MustSearchWithMainFromFormat4()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();

            foreach (var data in datas)
            {
                data.Name = null;
            }
            
            
            Bulk(datas);
            
            ElasticClient.Indices.Refresh();
            
            //When
            var results = (from i in Sut 
                    where i.Name == null
                    orderby i.Date, i.Age
                    select i)
                .Skip(5).Take(3).Select(x => new {x.Id, x.Age});
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(3);
        }
    }
}