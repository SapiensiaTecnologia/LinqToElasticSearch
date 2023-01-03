using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using AutoFixture;
using LinqToElasticSearch.IntegrationTests.Utils;
using Nest;

namespace LinqToElasticSearch.IntegrationTests
{
    public abstract class IntegrationTestsBase<T> : IDisposable where T : class
    {
        protected readonly ElasticClient ElasticClient;
        protected readonly ElasticQueryable<T> Sut;
        private readonly string IndexName = $"mlinqtoelasticsearch-${typeof(T).Name.ToLower()}-${Guid.NewGuid()}";
        protected Fixture Fixture { get; }

        protected IntegrationTestsBase()
        {
            Fixture = new Fixture();
            Fixture.Customize(new CustomeGeo());

            var server = GetSettingsValue("ElasticSearch.ReadModelNodeList", "http://localhost:9200");
            var username = GetSettingsValue("ElasticSearch.UserName", "");
            var password = GetSettingsValue("ElasticSearch.Password", "");
            
            ElasticClient = ElasticClientFactory.CreateElasticClient(server, username, password);
            
            if (ElasticClient.Indices.Exists(IndexName).Exists)
            {
                ElasticClient.Indices.Delete(IndexName);
            }

            ElasticClient.Indices.Create(IndexName, d => d.Settings(descriptor => descriptor)
                .Map<SampleData>(m => m.AutoMap()
                    .Properties(p=>p.GeoPoint(g=>g.Name(n=>n.PointGeo)))));
            
            Sut = new ElasticQueryable<T>(ElasticClient, IndexName);
            
        }

        protected void Bulk(IEnumerable<T> datas)
        {
            var response = ElasticClient.Bulk(descriptor => descriptor.Index(IndexName).IndexMany(datas));
            if (response.Errors)
            {
                
            }
        }
        
        protected void Index(T data)
        {
            ElasticClient.Index(data, descriptor => descriptor.Index(IndexName));
        }
        
        public void Dispose()
        {
            ElasticClient.Indices.Delete(IndexName); 
        }

        private string GetSettingsValue(string key, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }
    }
    
    public class CustomeGeo: ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<SampleData>(c => c.With(data => data.PointGeo, new GeoLocation(45,56)));
        }
    }
}