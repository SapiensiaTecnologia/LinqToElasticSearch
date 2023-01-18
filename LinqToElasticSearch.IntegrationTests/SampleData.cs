using System;
using System.Collections.Generic;
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
        public SampleType? EnumNullable { get; set; }
        public Guid? FolderId { get; set; }
        public Guid TypeId { get; set; }
        public bool Can { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public TimeSpan? TimeSpanNullable { get; set; }
        [Keyword] public IList<string> Emails { get; set; }
        [StringEnum] [Keyword] public SampleType SampleTypePropertyString { get; set; }
    }

    public enum SampleType
    {
        Sample,
        Type,
        SampleType
    }
}