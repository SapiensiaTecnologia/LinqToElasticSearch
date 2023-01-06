using System;
using Elasticsearch.Net;
using Nest;

namespace LinqToElasticSearch.IntegrationTests
{
    public class SampleData
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public DateTime Date { get; set; }
        
        public DateTime? Date1 { get; set; }
        
        public SampleType SampleTypeProperty { get; set; }
        
        [StringEnum]
        [Keyword]
        public SampleType SampleTypePropertyString { get; set; }
        
        public bool Can { get; set; }
    }

    public enum SampleType
    {
        Sample,
        Type,
        SampleType
    }
}