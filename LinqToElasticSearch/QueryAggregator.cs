using System;
using System.Collections.Generic;
using Nest;
using Remotion.Linq.Clauses;

namespace LinqToElasticSearch
{
    public class QueryAggregator
    {
        public List<QueryContainer> QueryContainers = new List<QueryContainer>();
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public List<string> PropertiesToSelect = new List<string>();
        public List<OrderProperties> OrderByExpressions = new List<OrderProperties>();
        public GroupProperty GroupByExpression;
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
    
    public class GroupProperty
    {
        public readonly Type PropertyType;
        public string PropertyName { get; set; }

        public GroupProperty(string propertyName, Type propertyType)
        {
            PropertyType = propertyType;
            PropertyName = propertyName;
        }
        
        public string GetKeywordIfNecessary()
        {
            return PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }
}