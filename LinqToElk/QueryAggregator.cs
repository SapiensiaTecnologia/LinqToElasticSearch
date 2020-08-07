﻿﻿using System;
  using System.Collections.Generic;
using Nest;
  using Remotion.Linq.Clauses;

  namespace LinqToElk
{
    public class QueryAggregator
    {
        public List<QueryContainer> QueryContainers = new List<QueryContainer>();
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public OrderProperties OrderBy { get; set; }
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
            return GetElasticType().ToLowerInvariant() == "text" ? ".keyword" : "";
        }

        private string GetElasticType()
        {
            switch (PropertyType.Name.ToLower())
            {
                case "datetime" :
                case "datetimeoffset" :
                    return "Date";
                case "bool":
                case "boolean":
                    return "boolean";
                case "int":
                case "int32":
                case "long":
                case "int64":
                case "float":
                case "single":
                case "decimal":
                case "double":
                    return "number";
                default:
                    return "Text";
            }
        }
    }
}