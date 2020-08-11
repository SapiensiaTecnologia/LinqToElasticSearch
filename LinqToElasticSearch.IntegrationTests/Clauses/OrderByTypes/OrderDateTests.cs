using System;
using System.Linq;
using AutoFixture;
using LinqToElasticSearch.Extensions;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.OrderByTypes
{
    public class OrderDateTests: IntegrationTestsBase<SampleData>
    {
        public OrderDateTests()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(11).ToList();
            foreach (var data in datas)
            {
                data.Date = DateTime.Now;
            }

            datas[7].Date = DateTime.Now.AddDays(-2);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
        }
        
        
        [Fact]
        public void OrderAscNumber()
        {
            //When
            var results = Sut.OrderBy(x => x.Date).ToList();

            //Then
            results.Count().Should().Be(11);
            results.First().Date.GetBeginOfDay().Should().Be(DateTime.Now.AddDays(-2).GetBeginOfDay());
        }
        
        [Fact]
        public void OrderDescNumber()
        {
            //When
            var results = Sut.OrderByDescending(x => x.Date).ToList();

            //Then
            results.Count().Should().Be(11);
            results[10].Date.GetBeginOfDay().Should().Be(DateTime.Now.AddDays(-2).GetBeginOfDay());
        }
    }
}