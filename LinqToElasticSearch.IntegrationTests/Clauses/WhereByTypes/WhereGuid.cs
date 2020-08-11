using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
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
        
        [Fact]
        public void WhereGuidNullableEqual()
        {
            //Given
            var datas = Fixture.CreateMany<SampleData>().ToList();
            foreach (var data in datas)
            {
                data.Id = Guid.NewGuid();
            }

            datas[1].Id = null;
            
            Bulk(datas);
            ElasticClient.Indices.Refresh();
            
            //When
            var results = Sut.Where(x => x.Id == null);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Id.Should().BeNull();
        }

        [Fact]
        public void WhereGuidTwice()
        {
            //Given
            var guids = Fixture.CreateMany<Guid>(5).ToList();

            var datas = Fixture.CreateMany<SampleData>(5).ToList();
            var count = 0;
            foreach (var data in datas)
            {
                data.Id = guids[count];
                count++;
            }

            datas[1].Id = null;

            Bulk(datas);
            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.Id != null).Where(x =>
                x.Id == guids[0] ||
                x.Id == guids[2] ||
                x.Id == guids[3] ||
                x.Id == guids[4]);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(4);
            listResults[0].Id.Should().Be(guids[0]);
            listResults[1].Id.Should().Be(guids[2]);
            listResults[2].Id.Should().Be(guids[3]);
            listResults[3].Id.Should().Be(guids[4]);
        }
    }
}