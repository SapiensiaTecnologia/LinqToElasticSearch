using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using LinqToElasticSearch.IntegrationTests.Utils;
using Nest;
using Newtonsoft.Json;

namespace LinqToElasticSearch.IntegrationTests
{
    public abstract class IntegrationTestsBase<T> : IDisposable where T : class
    {
        protected readonly ElasticClient ElasticClient;
        protected readonly ElasticQueryable<T> Sut;
        private readonly string IndexName = $"linqtoelasticsearch-{typeof(T).Name.ToLower()}-{Guid.NewGuid()}";
        protected Fixture Fixture { get; }

        protected IntegrationTestsBase()
        {
            Fixture = new Fixture();

            var server = GetSettingsValue("ElasticSearch.ReadModelNodeList", "http://localhost:9200");
            var username = GetSettingsValue("ElasticSearch.UserName", "");
            var password = GetSettingsValue("ElasticSearch.Password", "");
            
            ElasticClient = ElasticClientFactory.CreateElasticClient(server, username, password);
            
            if (ElasticClient.Indices.Exists(IndexName).Exists)
            {
                ElasticClient.Indices.Delete(IndexName);
            }

            ElasticClient.Indices.Create(IndexName, d => d.Settings(descriptor => descriptor).Map(m =>
            {
                m.Properties(prop => prop
                    .Number(sprop =>
                    {
                        sprop.Type(NumberType.Integer);
                        return sprop.Name("age");
                    }));
                
                m.Properties(prop => prop
                    .Boolean(sprop => sprop.Name("can")));
                
                m.Properties(prop => prop
                        .Text(sprop =>
                        {
                            sprop.Fields(f => 
                                f.Keyword(k => k.IgnoreAbove(256).Name("keyword")));
                            return sprop.Name("countryCode");
                        }));

                m.Properties(prop => prop
                    .Date(sprop => sprop.Name("date")));
                
                m.Properties(prop => prop
                    .Date(sprop => sprop.Name("date1")));

                m.Properties(prop => prop
                    .Keyword(sprop => sprop.Name("emails")));

                m.Properties(prop => prop
                    .Number(sprop => sprop.Name("enumNullable")));
                          
                m.Properties(prop => prop
                    .Keyword(sprop => sprop.Name("folderId")));

                m.Properties(prop => prop
                    .Keyword(sprop => sprop.Name("id")));

                m.Properties(prop => prop
                    .Text(sprop =>
                    {
                        sprop.Fields(f => 
                            f.Keyword(k => k.IgnoreAbove(256).Name("keyword")));
                        return sprop.Name("lastName");
                    }));

                m.Properties(prop => prop
                    .Text(sprop =>
                    {
                        sprop.Fields(f =>
                            f.Keyword(k => k.IgnoreAbove(256).Name("keyword")));
                        return sprop
                            .Name("name");
                    }));
                        
                
                m.Properties(prop => prop
                    .Number(sprop =>
                    {
                        sprop.Type(NumberType.Integer);
                        return sprop.Name("sampleTypeProperty");
                    }));
                
                m.Properties(prop => prop
                    .Keyword(sprop => sprop.Name("sampleTypePropertyString")));

                m.Properties(prop => prop
                    .Number(sprop =>
                    {
                        sprop.Type(NumberType.Long);
                        return sprop.Name("timeSpan");
                    }));
                
                m.Properties(prop => prop
                    .Number(sprop =>
                    {
                        sprop.Type(NumberType.Long);
                        return sprop.Name("timeSpanNullable");
                    }));

                m.Properties(prop => prop
                    .Keyword(sprop => sprop.Name("typeId")));
                
                return m;
            }));
            
            Sut = new ElasticQueryable<T>(ElasticClient, IndexName);
            
        }

        protected void Bulk(IEnumerable<T> datas)
        {
            ElasticClient.Bulk(descriptor => descriptor.Index(IndexName).IndexMany(datas));
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

        public void CompareAsJson(object obj1, object obj2)
        {
            JsonConvert.SerializeObject(obj1).Should().Be(JsonConvert.SerializeObject(obj2));
        }
    }
}