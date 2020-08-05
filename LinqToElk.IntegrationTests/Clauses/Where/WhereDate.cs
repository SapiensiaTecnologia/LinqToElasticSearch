using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElk.IntegrationTests.Clauses.Where
{
    public class WhereDate: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereDateGreaterThan()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTime.Now.AddHours(10);
            datas[0].Date = DateTime.Now.AddHours(9); 
            datas[1].Date = date;
            datas[2].Date = DateTime.Now.AddHours(11);
    
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date > date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Date.Should().Be(datas[2].Date);
        }
        
        [Fact]
        public void WhereDateGreaterThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTime.Now.AddHours(10);
            datas[0].Date = DateTime.Now.AddHours(9); 
            datas[1].Date = date;
            datas[2].Date = DateTime.Now.AddHours(11);
    
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date >= date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Date.Should().Be(datas[1].Date);
            listResults[1].Date.Should().Be(datas[2].Date);
        }
        
        [Fact]
        public void WhereDateLessThan()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTime.Now.AddHours(10);
            datas[0].Date = DateTime.Now.AddHours(9); 
            datas[1].Date = date;
            datas[2].Date = DateTime.Now.AddHours(11);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date < date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Date.Should().Be(datas[0].Date);
        }
        
        [Fact]
        public void WhereDateLessThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTime.Now.AddHours(10);
            datas[0].Date = DateTime.Now.AddHours(9); 
            datas[1].Date = date;
            datas[2].Date = DateTime.Now.AddHours(11); 
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date <= date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Date.Should().Be(datas[0].Date);
            listResults[1].Date.Should().Be(datas[1].Date);
        }
        
        [Fact]
        public void WhereDateEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.Date = DateTime.Now.AddHours(-10);
            }
    
            var date = DateTime.Now.AddHours(10);
            datas[1].Date = date;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date == date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Date.Should().Be(datas[1].Date);
        }
        
        [Fact]
        public void WhereDateNotEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.Date = DateTime.Now.AddHours(-10);
            }
    
            var date = DateTime.Now.AddHours(10);
            datas[1].Date = date;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date != date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Date.Should().Be(datas[0].Date);
            listResults[1].Date.Should().Be(datas[2].Date);
        }
    }
}