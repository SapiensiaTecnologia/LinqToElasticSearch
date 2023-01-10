using System;
using System.Collections.Generic;
using Nest;
using Remotion.Linq.Clauses;

namespace LinqToElasticSearch
{
    public class QueryAggregator
    {
        public QueryContainer Query { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public List<string> PropertiesToSelect = new List<string>();
        public List<OrderProperties> OrderByExpressions = new List<OrderProperties>();
        public List<GroupByProperties> GroupByExpressions = new List<GroupByProperties>();
    }

    public class OrderProperties
    {
        public readonly Type PropertyType;

        public string PropertyName { get; set; }
        public OrderingDirection OrderingDirection { get; set; }

        public OrderProperties(string propertyName, Type propertyType, OrderingDirection direction)
        {
            PropertyType = propertyType;
            PropertyName = propertyName;
            OrderingDirection = direction;
        }

        public string GetKeywordIfNecessary()
        {
            return PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }

    public class GroupByProperties
    {
        public string PropertyName { get; }
        public Type PropertyType { get; set; }

        public GroupByProperties(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }
        
        public string GetKeywordIfNecessary()
        {
            return PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }
}