﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
  using LinqToElk.Extensions;
  using Nest;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElk
{
    public class ElasticQueryExecutor<TU> : IQueryExecutor where TU : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _dataId;
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;

        public ElasticQueryExecutor(IElasticClient elasticClient, string dataId)
        {
            _elasticClient = elasticClient;
            _dataId = dataId;
            _propertyNameInferrerParser = new PropertyNameInferrerParser(_elasticClient);
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            
            var queryAggregator = ElasticGeneratorQueryModelVisitor.GenerateElasticQuery(queryModel, _propertyNameInferrerParser);
            
            var documents= _elasticClient.Search<TU>(descriptor =>
            {
                descriptor.Index(_dataId);


                if (queryAggregator.Skip != null)
                {
                    descriptor.From(queryAggregator.Skip);
                }
                else
                {
                    descriptor.Size(10000);
                }

                if (queryAggregator.Take != null)
                {
                    descriptor.Take(queryAggregator.Take);
                    descriptor.Size(queryAggregator.Take);
                }
                
                if (queryAggregator.QueryContainers.Any())
                {
                    descriptor.Query(q => q.Bool(x => x.Must(queryAggregator.QueryContainers.ToArray())));
                }
                else
                {
                    descriptor.MatchAll();
                }


                if (queryAggregator.OrderBy != null)
                {
                    if (queryAggregator.OrderBy.OrderingDirection == OrderingDirection.Asc)
                    {
                        var property = _propertyNameInferrerParser.Parser(queryAggregator.OrderBy.Property);
                        descriptor.Sort(d => d.Ascending(new Field(property 
                            // +  ".keyword"
                        )));
                    }
                    else
                    {
                        var property = _propertyNameInferrerParser.Parser(queryAggregator.OrderBy.Property);
                        descriptor.Sort(d => d.Descending(new Field(property 
                            // + ".keyword"
                        )));
                    }
                }
                
                return descriptor;

            }).Documents;

            return (IEnumerable<T>) documents;
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var sequence = ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
        }

        public T ExecuteScalar<T>(QueryModel queryModel)                
        {
            var queryAggregator = ElasticGeneratorQueryModelVisitor.GenerateElasticQuery(queryModel, _propertyNameInferrerParser);

            foreach (var resultOperator in queryModel.ResultOperators)
            {
                if (resultOperator is CountResultOperator)
                {
                    var result = _elasticClient.Count<TU>(descriptor =>
                    {
                        descriptor.Index(_dataId);
                    
                        if (queryAggregator.QueryContainers.Any())
                        {
                            descriptor.Query(q => q.Bool(x => x.Must(queryAggregator.QueryContainers.ToArray())));
                        }
                        return descriptor;
                    }).Count;
            
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return (T)converter.ConvertFromString(result.ToString());
                    }
                }
            }
            
            return default(T);
        }
    }
}