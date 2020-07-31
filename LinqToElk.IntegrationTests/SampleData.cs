using System;

namespace LinqToElk
{
    public class SampleData
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public DateTime Date { get; set; }
        
        public SampleType SampleType { get; set; }
    }

    public enum SampleType
    {
        Sample,
        Type,
        SampleType
    }
}