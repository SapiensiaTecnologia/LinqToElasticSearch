using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using Nest;
using Remotion.Linq.Clauses;

namespace LinqToElasticSearch
{
    public class QueryAggregator
    {
        public QueryContainer Query { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public List<SelectProperties> SelectExpressions = new List<SelectProperties>();
        public List<OrderProperties> OrderByExpressions = new List<OrderProperties>();
        public List<GroupByProperties> GroupByExpressions = new List<GroupByProperties>();

        private bool? _usesAggregationFunction = null;
        public bool UsesAggregationFunction
        {
            get
            {
                if (_usesAggregationFunction == null)
                {
                    _usesAggregationFunction = SelectExpressions.Any(x =>
                        x.ProjectionType == ProjectionType.Count ||
                        x.ProjectionType == ProjectionType.Max ||
                        x.ProjectionType == ProjectionType.Min);
                }

                return _usesAggregationFunction.Value;
            }
        }
    }

    public class SelectProperties
    {
        public int PropertyIndex { get; set; }
        public Type PropertyType { get; private set; }
        public string PropertyName { get; private set; }
        public ProjectionType ProjectionType { get; private set; }
        public string ElasticFieldName { get; private set; }

        public SelectProperties(string elasticFieldName, string propertyName, Type propertyType, int propertyIndex, ProjectionType projectionType)
        {
            ElasticFieldName = elasticFieldName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            PropertyIndex = propertyIndex;
            ProjectionType = projectionType;
        }
    }

    public enum ProjectionType
    {
        Property,
        Document,
        Count,
        Max,
        Min
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
        public string ElasticFieldName { get; set; }
        
        public GroupByProperties(string elasticFieldName, string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            ElasticFieldName = elasticFieldName;
        }

        public string GetKeywordIfNecessary()
        {
            return PropertyType.Name.ToLower().Contains("string") ? ".keyword" : "";
        }
    }
}