using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LinqToElasticSearch.IntegrationTests.Clauses.WhereByTypes
{
    public class WhereTimeTests : IntegrationTestsBase<SampleData>
    {
        [Fact]
        public void WhereTimeEqual()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();
            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan == samples[1].TimeSpan);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Id.Should().Be(samples[1].Id);
        }

        [Fact]
        public void WhereTimeNotEqual()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();
            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan != samples[1].TimeSpan);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(2);
            listResults[0].Id.Should().Be(samples[0].Id);
            listResults[1].Id.Should().Be(samples[2].Id);
        }

        [Fact]
        public void WhereTimeGreaterThen()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();

            var timeSpan = Fixture.Create<TimeSpan>();

            samples[0].TimeSpan = timeSpan;
            samples[1].TimeSpan = timeSpan.Add(TimeSpan.FromMinutes(5));
            samples[2].TimeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(5));

            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan > timeSpan).ToList();

            //Then
            results.Count.Should().Be(1);
            results[0].TimeSpan.Should().Be(samples[1].TimeSpan);
        }

        [Fact]
        public void WhereTimeGreaterThanOrEqual()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();

            var timeSpan = Fixture.Create<TimeSpan>();

            samples[0].TimeSpan = timeSpan;
            samples[1].TimeSpan = timeSpan.Add(TimeSpan.FromMinutes(5));
            samples[2].TimeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(5));

            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan >= timeSpan).ToList();

            //Then
            results.Count.Should().Be(2);
            results[0].TimeSpan.Should().Be(samples[0].TimeSpan);
            results[1].TimeSpan.Should().Be(samples[1].TimeSpan);
        }

        [Fact]
        public void WhereTimeLessThan()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();

            var timeSpan = Fixture.Create<TimeSpan>();

            samples[0].TimeSpan = timeSpan;
            samples[1].TimeSpan = timeSpan.Add(TimeSpan.FromMinutes(5));
            samples[2].TimeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(5));

            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan < timeSpan).ToList();

            //Then
            results.Count.Should().Be(1);
            results[0].TimeSpan.Should().Be(samples[2].TimeSpan);
        }

        [Fact]
        public void WhereTimeLessThanOrEqual()
        {
            //Given
            var samples = Fixture.CreateMany<SampleData>().ToList();

            var timeSpan = Fixture.Create<TimeSpan>();

            samples[0].TimeSpan = timeSpan;
            samples[1].TimeSpan = timeSpan.Add(TimeSpan.FromMinutes(5));
            samples[2].TimeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(5));

            Bulk(samples);

            ElasticClient.Indices.Refresh();

            //When
            var results = Sut.Where(x => x.TimeSpan <= timeSpan).ToList();

            //Then
            results.Count.Should().Be(2);
            results[0].TimeSpan.Should().Be(samples[0].TimeSpan);
            results[1].TimeSpan.Should().Be(samples[2].TimeSpan);
        }
    }
}