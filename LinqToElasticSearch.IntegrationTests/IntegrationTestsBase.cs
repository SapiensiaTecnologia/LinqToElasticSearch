﻿using System;
using System.Collections.Generic;
using AutoFixture;
using LinqToElasticSearch.IntegrationTests.Utils;
using Nest;

namespace LinqToElasticSearch.IntegrationTests
{
    public abstract class IntegrationTestsBase<T> : IDisposable where T : class
    {
        protected readonly ElasticClient ElasticClient;
        protected readonly ElasticQueryable<T> Sut;
        private readonly string IndexName = $"linqtoelasticsearch-${typeof(T).Name.ToLower()}-${Guid.NewGuid()}";
        protected Fixture Fixture { get; }

        protected IntegrationTestsBase()
        {
            Fixture = new Fixture();
            
            ElasticClient = ElasticClientFactory.CreateElasticClient("http://localhost:9200", "", ""); 
            
            if (ElasticClient.Indices.Exists(IndexName).Exists)
            {
                ElasticClient.Indices.Delete(IndexName);
            }

            ElasticClient.Indices.Create(IndexName, d => d.Settings(descriptor => descriptor).Map(m => m.AutoMap<T>()));
            
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
    }
}