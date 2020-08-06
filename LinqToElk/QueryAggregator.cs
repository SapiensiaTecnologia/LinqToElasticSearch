﻿﻿using System.Collections.Generic;
using Nest;

namespace LinqToElk
{
    public class QueryAggregator
    {
        public List<QueryContainer> QueryContainers = new List<QueryContainer>();
        public int? Take { get; set; }
        public int? Skip { get; set; }
        
    }
}