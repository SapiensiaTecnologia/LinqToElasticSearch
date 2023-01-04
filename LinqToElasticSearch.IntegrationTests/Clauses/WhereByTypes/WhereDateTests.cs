using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LinqToElasticSearch.Extensions;
using Xunit;
using System.Linq.Dynamic.Core;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereDateTests: IntegrationTestsBase<SampleData>
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
        
        [Fact]
        public void WhereDateNotEqual2()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.Date = DateTime.Now.GetBeginOfDay();
            }
    
            var date = DateTime.Now.AddDays(1).GetBeginOfDay();
            datas[1].Date = date;

            var dateT = date.AddDays(1);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Date < date || x.Date >= dateT);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Date.Should().Be(datas[0].Date);
            listResults[1].Date.Should().Be(datas[2].Date);
        }

        [Fact]
        public void WhereDateRangeWithDifferentOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].Date = DateTime.Now.AddDays(-2);
            datas[0].Name = "abcdef";
            datas[1].Date = DateTime.Now.AddDays(-1);
            datas[1].Name = "abcdef";
            datas[2].Date = DateTime.Now;
            datas[2].Name = "Hello World";

            var dateFilterBeginOfDate = datas[1].Date.GetBeginOfDay().ToString("O");
            var dateFilterEndOfDate = datas[1].Date.GetEndOfDay().ToString("O");
            var stringFilter = datas[1].Name;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    $"Name=\"{stringFilter}\" && (Date<\"{dateFilterBeginOfDate}\" || Date>\"{dateFilterEndOfDate}\")").ToList();
            
            results.Should().HaveCount(1);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].Date.Should().Be(datas[0].Date);
        }

        [Fact]
        public void WhereDateRangeWithEqualsOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].Date = DateTime.Parse("2018-03-22");
            datas[0].Name = "abcdef";
            datas[1].Date = DateTime.Parse("2018-03-22");
            datas[1].Name = "abcdef";
            datas[2].Date = DateTime.Now;
            datas[2].Name = "Hello World";
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    "Name=\"abcdef\" && (Date>=DateTime(\"2018-03-22T00:00:00.0000000\") && Date<=DateTime(\"2018-03-22T23:59:59.9999999\"))").ToList();
            
            results.Should().HaveCount(2);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].Date.Should().Be(datas[0].Date);
            
            results[1].Id.Should().Be(datas[1].Id);
            results[1].Name.Should().Be(datas[1].Name);
            results[1].Date.Should().Be(datas[1].Date);
        }
        
        [Fact]
        public void WhereDateNullableRangeWithEqualsOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].Date1 = DateTime.Parse("2018-03-22");
            datas[0].Name = "abcdef";
            datas[1].Date1 = DateTime.Parse("2018-03-22");
            datas[1].Name = "abcdef";
            datas[2].Date1 = DateTime.Now;
            datas[2].Name = "Hello World";
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    "Name=\"abcdef\" && (Date1>=DateTime(\"2018-03-22T00:00:00.0000000\") && Date1<=DateTime(\"2018-03-22T23:59:59.9999999\"))").ToList();
            
            results.Should().HaveCount(2);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].Date.Should().Be(datas[0].Date);
            results[0].Date1.Should().Be(datas[0].Date1);
            
            results[1].Id.Should().Be(datas[1].Id);
            results[1].Name.Should().Be(datas[1].Name);
            results[1].Date.Should().Be(datas[1].Date);
            results[1].Date1.Should().Be(datas[1].Date1);
        }
        
    }
}