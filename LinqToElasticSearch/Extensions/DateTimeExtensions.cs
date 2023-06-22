using System;

namespace LinqToElasticSearch.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeSpan FullDayTime = TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1));

        public static DateTime GetBeginOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        public static DateTime GetEndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.Add(FullDayTime);
        }
        
        public static DateTimeOffset GetBeginOfDay(this DateTimeOffset dateTime)
        {
            return dateTime.Date;
        }

        public static DateTimeOffset GetEndOfDay(this DateTimeOffset dateTime)
        {
            return dateTime.Date.Add(FullDayTime);
        }
    }
}