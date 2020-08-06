﻿﻿using System.Collections.Generic;
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
        public OrderProperties(string property, OrderingDirection direction)
        {
            Property = property;
            OrderingDirection = direction;
        }

        public string Property { get; set; }
        public OrderingDirection OrderingDirection { get; set; }
    }
}