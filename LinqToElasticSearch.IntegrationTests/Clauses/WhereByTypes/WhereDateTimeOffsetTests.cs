using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LinqToElasticSearch.Extensions;
using Xunit;
using System.Linq.Dynamic.Core;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereDateTimeOffsetOffsetTests: IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereDateGreaterThan()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTimeOffset.Now.AddHours(10);
            datas[0].DateOffset = DateTimeOffset.Now.AddHours(9); 
            datas[1].DateOffset = date;
            datas[2].DateOffset = DateTimeOffset.Now.AddHours(11);
    
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset > date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].DateOffset.Should().Be(datas[2].DateOffset);
        }
        
        [Fact]
        public void WhereDateGreaterThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTimeOffset.Now.AddHours(10);
            datas[0].DateOffset = DateTimeOffset.Now.AddHours(9); 
            datas[1].DateOffset = date;
            datas[2].DateOffset = DateTimeOffset.Now.AddHours(11);
    
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset >= date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].DateOffset.Should().Be(datas[1].DateOffset);
            listResults[1].DateOffset.Should().Be(datas[2].DateOffset);
        }
        
        [Fact]
        public void WhereDateLessThan()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTimeOffset.Now.AddHours(10);
            datas[0].DateOffset = DateTimeOffset.Now.AddHours(9); 
            datas[1].DateOffset = date;
            datas[2].DateOffset = DateTimeOffset.Now.AddHours(11);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset < date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].DateOffset.Should().Be(datas[0].DateOffset);
        }
        
        [Fact]
        public void WhereDateLessThanOrEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            var date = DateTimeOffset.Now.AddHours(10);
            datas[0].DateOffset = DateTimeOffset.Now.AddHours(9); 
            datas[1].DateOffset = date;
            datas[2].DateOffset = DateTimeOffset.Now.AddHours(11); 
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset <= date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].DateOffset.Should().Be(datas[0].DateOffset);
            listResults[1].DateOffset.Should().Be(datas[1].DateOffset);
        }
        
        [Fact]
        public void WhereDateEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.DateOffset = DateTimeOffset.Now.AddHours(-10);
            }
    
            var date = DateTimeOffset.Now.AddHours(10);
            datas[1].DateOffset = date;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset == date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].DateOffset.Should().Be(datas[1].DateOffset);
        }
        
        [Fact]
        public void WhereDateNotEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.DateOffset = DateTimeOffset.Now.AddHours(-10);
            }
    
            var date = DateTimeOffset.Now.AddHours(10);
            datas[1].DateOffset = date;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset != date);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].DateOffset.Should().Be(datas[0].DateOffset);
            listResults[1].DateOffset.Should().Be(datas[2].DateOffset);
        }
        
        [Fact]
        public void WhereDateNotEqual2()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.DateOffset = DateTimeOffset.Now.GetBeginOfDay();
            }
    
            var date = DateTimeOffset.Now.AddDays(1).GetBeginOfDay();
            datas[1].DateOffset = date;

            var dateT = date.AddDays(1);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.DateOffset < date || x.DateOffset >= dateT);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].DateOffset.Should().Be(datas[0].DateOffset);
            listResults[1].DateOffset.Should().Be(datas[2].DateOffset);
        }

        [Fact]
        public void WhereDateRangeWithDifferentOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].DateOffset = DateTimeOffset.Now.AddDays(-2);
            datas[0].Name = "abcdef";
            datas[1].DateOffset = DateTimeOffset.Now.AddDays(-1);
            datas[1].Name = "abcdef";
            datas[2].DateOffset = DateTimeOffset.Now;
            datas[2].Name = "Hello World";

            var dateFilterBeginOfDate = datas[1].DateOffset.GetBeginOfDay().ToString("O");
            var dateFilterEndOfDate = datas[1].DateOffset.GetEndOfDay().ToString("O");
            var stringFilter = datas[1].Name;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    $"Name=\"{stringFilter}\" && (DateOffset<\"{dateFilterBeginOfDate}\" || DateOffset>\"{dateFilterEndOfDate}\")").ToList();
            
            results.Should().HaveCount(1);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].DateOffset.Should().Be(datas[0].DateOffset);
        }

        [Fact]
        public void WhereDateRangeWithEqualsOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].DateOffset = DateTimeOffset.Parse("2018-03-22");
            datas[0].Name = "abcdef";
            datas[1].DateOffset = DateTimeOffset.Parse("2018-03-22");
            datas[1].Name = "abcdef";
            datas[2].DateOffset = DateTimeOffset.Now;
            datas[2].Name = "Hello World";
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    "Name=\"abcdef\" && (DateOffset>=DateTimeOffset(\"2018-03-22T00:00:00.0000000\") && DateOffset<=DateTimeOffset(\"2018-03-22T23:59:59.9999999\"))").ToList();
            
            results.Should().HaveCount(2);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].DateOffset.Should().Be(datas[0].DateOffset);
            
            results[1].Id.Should().Be(datas[1].Id);
            results[1].Name.Should().Be(datas[1].Name);
            results[1].DateOffset.Should().Be(datas[1].DateOffset);
        }
        
        [Fact]
        public void WhereDateNullableRangeWithEqualsOperator()
        {
            // Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            datas[0].DateOffset1 = DateTimeOffset.Parse("2018-03-22");
            datas[0].Name = "abcdef";
            datas[1].DateOffset1 = DateTimeOffset.Parse("2018-03-22");
            datas[1].Name = "abcdef";
            datas[2].DateOffset1 = DateTimeOffset.Now;
            datas[2].Name = "Hello World";
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.AsQueryable()
                .Where(
                    "Name=\"abcdef\" && (DateOffset1>=DateTimeOffset(\"2018-03-22T00:00:00.0000000\") && DateOffset1<=DateTimeOffset(\"2018-03-22T23:59:59.9999999\"))").ToList();
            
            results.Should().HaveCount(2);
            results[0].Id.Should().Be(datas[0].Id);
            results[0].Name.Should().Be(datas[0].Name);
            results[0].DateOffset.Should().Be(datas[0].DateOffset);
            results[0].DateOffset1.Should().Be(datas[0].DateOffset1);
            
            results[1].Id.Should().Be(datas[1].Id);
            results[1].Name.Should().Be(datas[1].Name);
            results[1].DateOffset.Should().Be(datas[1].DateOffset);
            results[1].DateOffset1.Should().Be(datas[1].DateOffset1);
        }
        
    }
}