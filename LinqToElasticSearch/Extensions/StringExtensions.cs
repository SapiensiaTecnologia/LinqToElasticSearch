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
        /// <summary>
        /// Determine inf location is in distance range
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other">other location to compare</param>
        /// <param name="distance">distance in kilometers</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool Distance(this GeoLocation @this, GeoLocation other, double distance)
        {
            throw new NotImplementedException();
        }
    }

}