using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LinqToElasticSearch.Extensions;
using Nest;
using NetTopologySuite.Geometries;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.GroupByTypes
{
    
    public class GroupByBoolTests : IntegrationTestsBase<SampleData>
    {
        
        [Fact]
        public void GroupByBoolWithUniqueGrouping()
        {

            try
            {
                //Given
                var datas = Fixture.CreateMany<SampleData>(3).ToList();
            
                datas[0].Can = true;
                datas[1].Can = true;
                datas[2].Can = false;
            
                Bulk(datas);
                ElasticClient.Indices.Refresh();
            
                //When
                var r = Sut.Where(x => x.PointGeo.Distance(new GeoLocation(34, 67), 100)>100).ToList();
                var results = Sut.GroupBy(x => x.Can).ToList();
            
                //Then
                results.Count.Should().Be(2);
                results.Should().ContainSingle(x =>
                    x.Key == true
                    && x.Count() == 2);
            
                results.Should().ContainSingle(x =>
                    x.Key == false
                    && x.Count() == 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [Fact]
        public void GroupByBoolWithWithMultipleGrouping()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>(4).ToList();
            
            datas[0].Can = true;
            datas[0].Age = 3;
            
            datas[1].Can = true;
            datas[1].Age = 3;
            
            datas[2].Can = true;
            datas[2].Age = 4;
            
            datas[3].Can = false;
            datas[3].Age = 5;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.GroupBy(x => new {x.Can, x.Age}).ToList();
            
            //Then
            results.Count.Should().Be(3);
            results.Should().ContainSingle(x =>
                x.Key.Can == true
                && x.Key.Age == 3
                && x.Count() == 2);
            
            results.Should().ContainSingle(x =>
                x.Key.Can == true
                && x.Key.Age == 4
                && x.Count() == 1);
            
            results.Should().ContainSingle(x =>
                x.Key.Can == false
                && x.Key.Age == 5
                && x.Count() == 1);
            
        }
        
        
    }
}