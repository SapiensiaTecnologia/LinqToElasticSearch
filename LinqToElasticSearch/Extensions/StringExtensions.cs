using System;

namespace LinqToElasticSearch.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, string value,  StringComparison comparison)
        {
            return str.IndexOf(value, comparison) >= 0;
        }
    }
}