using System;
using System.Collections.Generic;
using AutoFixture;
using LinqToElk.IntegrationTests.Utils;
using Nest;

namespace LinqToElk.IntegrationTests
{
    public abstract class IntegrationTestsBase<T>: IDisposable where T : class 
    {
        protected readonly ElasticClient ElasticClient;
        protected readonly ElasticQueryable<T> Sut;
        private readonly string _dataId = Guid.NewGuid().ToString();
        protected Fixture Fixture { get; }

        protected IntegrationTestsBase()
        {
            Fixture = new Fixture();
            
            ElasticClient = ElasticClientFactory.CreateElasticClient("http://localhost:9200", "", ""); 
            
            if (ElasticClient.Indices.Exists(_dataId).Exists)
            {
                ElasticClient.Indices.Delete(_dataId);
            }
            
            Sut = new ElasticQueryable<T>(ElasticClient, _dataId);
        }

        protected void Bulk(IEnumerable<T> datas)
        {
            ElasticClient.Bulk(descriptor => descriptor.Index(_dataId).IndexMany(datas));
        }
        
        protected void Index(T data)
        {
            ElasticClient.Index(data, descriptor => descriptor.Index(_dataId));
        }


        public void Dispose()
        {
            ElasticClient.Indices.Delete(_dataId); 
        }
    }
}