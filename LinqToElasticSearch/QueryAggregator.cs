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
        private readonly bool _isKeyword;

        public string PropertyName { get; set; }
        public OrderingDirection OrderingDirection { get; set; }

        public OrderProperties(string propertyName, Type propertyType, OrderingDirection direction, bool isKeyword)
        {
            PropertyType = propertyType;
            _isKeyword = isKeyword;
            PropertyName = propertyName;
            OrderingDirection = direction;
        }

        public string GetKeywordIfNecessary()
        {
            return _isKeyword || PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }

    public class GroupByProperties
    {
        private readonly bool _isKeyword;
        public string PropertyName { get; }
        public Type PropertyType { get; set; }

        public GroupByProperties(string propertyName, Type propertyType, bool isKeyword)
        {
            _isKeyword = isKeyword;
            PropertyName = propertyName;
            PropertyType = propertyType;
        }
        
        public string GetKeywordIfNecessary()
        {
            return _isKeyword || PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }
}