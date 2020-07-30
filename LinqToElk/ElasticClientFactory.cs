using System;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Nest;

namespace LinqToElk
{
    public static class ElasticClientFactory
    {
        public static ElasticClient CreateElasticClient(string url, string user, string password)
        {
            const string nodesDefault = "http://localhost:9200";

            var nodes = new[] {new Uri(url)};
            var connectionSettings = new ConnectionSettings(new StaticConnectionPool(nodes));
            
            connectionSettings.BasicAuthentication(user, password);

            connectionSettings.EnableDebugMode(details =>
            {
                Console.WriteLine($"ES Request: {Encoding.UTF8.GetString(details.RequestBodyInBytes ?? new byte[0])}");
                Console.WriteLine($"ES Response: {Encoding.UTF8.GetString(details.ResponseBodyInBytes ?? new byte[0])}");
            });
            
            return new ElasticClient(connectionSettings);
            
        }
    }
}