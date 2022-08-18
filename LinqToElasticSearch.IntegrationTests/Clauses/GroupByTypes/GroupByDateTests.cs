using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.GroupByTypes
{
    
    public class GroupByDateTests : IntegrationTestsBase<SampleData>
    {
        
        [Fact]
        public void GroupByDateWithUniqueGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(3).ToList();

            var date = Fixture.Create<DateTime>();
                       
            datas[0].Date = date;
            datas[1].Date = date;
            datas[2].Date = date.AddDays(1);
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.GroupBy(x => x.Date).ToList();
            
            //Then
            results.Count.Should().Be(2);
            results.Should().ContainSingle(x =>
                x.Key.Date == date.Date
                && x.Count() == 2);
            
            results.Should().ContainSingle(x =>
                x.Key.Date == date.AddDays(1).Date
                && x.Count() == 1);
        }

        [Fact]
        public void GroupByDateWithWithMultipleGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(6).ToList();
            
            var date = Fixture.Create<DateTime>();

            datas[0].Date = date;
            datas[0].SampleTypeProperty = SampleType.Sample;
            datas[0].Can = true;

            datas[1].Date = date;
            datas[1].SampleTypeProperty = SampleType.Sample;
            datas[1].Can = true;
            
            datas[2].Date = date;
            datas[2].SampleTypeProperty = SampleType.Type;
            datas[2].Can = true;

            datas[3].Date = date;
            datas[3].SampleTypeProperty = SampleType.Sample;
            datas[3].Can = false;

            datas[4].Date = date.AddDays(1);
            datas[4].SampleTypeProperty = SampleType.SampleType;
            datas[4].Can = false;
            
            datas[5].Date = date.AddDays(1);
            datas[5].SampleTypeProperty = SampleType.SampleType;
            datas[5].Can = true;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.GroupBy(x => new { x.Date, x.SampleTypeProperty, x.Can }).ToList();
            
            //Then
            results.Count.Should().Be(5);
            
            results.Should().ContainSingle(x =>
                x.Key.Date.Date == date.Date
                && x.Key.Can == true
                && x.Key.SampleTypeProperty == SampleType.Sample
                && x.Count() == 2);
            
            results.Should().ContainSingle(x =>
                x.Key.Date.Date == date.Date
                && x.Key.Can == true
                && x.Key.SampleTypeProperty == SampleType.Type
                && x.Count() == 1);
            
            results.Should().ContainSingle(x =>
                x.Key.Date.Date == date.Date
                && x.Key.Can == false
                && x.Key.SampleTypeProperty == SampleType.Sample
                && x.Count() == 1);
           
            results.Should().ContainSingle(x =>
                x.Key.Date.Date == date.AddDays(1).Date
                && x.Key.Can == false
                && x.Key.SampleTypeProperty == SampleType.SampleType
                && x.Count() == 1);
            
            results.Should().ContainSingle(x =>
                x.Key.Date.Date == date.AddDays(1).Date
                && x.Key.Can == true
                && x.Key.SampleTypeProperty == SampleType.SampleType
                && x.Count() == 1);
        }
        
        
    }
}