using System.Linq;
using FluentAssertions;
using Nest;
using Xunit;

namespace LinqToElk.IntegrationTests
{
    public class ElasticClientFactoryTests
    {
        private readonly ElasticClient _elasticClient;
        private ElasticQueryable<SampleDataSourceItem> _sut;
        private const string DataId = "123456789";

        public ElasticClientFactoryTests()
        {
            _elasticClient = ElasticClientFactory.CreateElasticClient("http://localhost:9200", "", "");
            
 
            if (_elasticClient.Indices.Exists(DataId).Exists)
            {
                _elasticClient.Indices.Delete(DataId);
            }
            _sut = new ElasticQueryable<SampleDataSourceItem>(_elasticClient, DataId);
        }
        
        [Fact]
        public void SelectQueryExpression()
        {
            //Given
            var sampleDataSourceItem = new SampleDataSourceItem()
            {
                Name = "Frai",
                Description = "This"
            };

            _elasticClient.Index(sampleDataSourceItem, descriptor => descriptor.Index(DataId));
            
            _elasticClient.Indices.Refresh();
            
            //When
            var results = from i in _sut select i;
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be("Frai");
        }
        
        [Fact]
        public void WhereQueryExpression()
        {
            //Given
            var sampleDataSourceItem = new SampleDataSourceItem()
            {
                Name = "Frai",
                Description = "This"
            };
            var sampleDataSourceItem2 = new SampleDataSourceItem()
            {
                Name = "NotFrai",
                Description = "This"
            };

            _elasticClient.Index(sampleDataSourceItem, descriptor => descriptor.Index(DataId));
            _elasticClient.Index(sampleDataSourceItem2, descriptor => descriptor.Index(DataId));
            
            _elasticClient.Indices.Refresh();
            
            //When
            var results = _sut.Where(x => "Frai" == x.Name);
            var listResults = results.ToList();

            //Then
            listResults.Count.Should().Be(1);
            listResults[0].Name.Should().Be("Frai");
        }
    }
}