using System;
using Nest;

namespace LinqToElasticSearch.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, string value,  StringComparison comparison)
        {
            return str.IndexOf(value, comparison) >= 0;
        }
    }
    
    
    public static class GeoExtensions
    {
        public static double Distance(this GeoLocation str, GeoLocation other, double distance)
        {
            throw new NotImplementedException();
        }
    }

}