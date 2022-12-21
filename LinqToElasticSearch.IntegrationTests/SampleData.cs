using System;
using Elasticsearch.Net;
using Nest;
using NetTopologySuite.Geometries;

namespace LinqToElasticSearch.IntegrationTests
{
    public class SampleData
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public DateTime Date { get; set; }
        
        public SampleType SampleTypeProperty { get; set; }
        
        [StringEnum]
        [Keyword]
        public SampleType SampleTypePropertyString { get; set; }
        
        public bool Can { get; set; }

        public GeoLocation  PointGeo { get; set; }
    }

    public class A
    {
        public string Test { get; set; }
    }

    public enum SampleType
    {
        Sample,
        Type,
        SampleType
    }
}