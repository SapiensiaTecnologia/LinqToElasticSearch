using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Nest;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElasticSearch
{
    public class ElasticQueryExecutor<K>: IQueryExecutor
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _dataId;
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;
        private readonly ElasticGeneratorQueryModelVisitor<K> _elasticGeneratorQueryModelVisitor;

        public ElasticQueryExecutor(IElasticClient elasticClient, string dataId)
        {
            _elasticClient = elasticClient;
            _dataId = dataId;
            _propertyNameInferrerParser = new PropertyNameInferrerParser(_elasticClient);
            _elasticGeneratorQueryModelVisitor = new ElasticGeneratorQueryModelVisitor<K>(_propertyNameInferrerParser);
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var queryAggregator = _elasticGeneratorQueryModelVisitor.GenerateElasticQuery<T>(queryModel);

            var documents= _elasticClient.Search<IDictionary<string, object>>(descriptor =>
            {
                descriptor.Index(_dataId);

                if (queryModel.SelectClause != null && queryModel.SelectClause.Selector is MemberExpression memberExpression)
                {
                    descriptor.Source(x => x.Includes(f => f.Field(_propertyNameInferrerParser.Parser(memberExpression.Member.Name))));
                }

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


                if (queryAggregator.OrderByExpressions.Any())
                {
                    descriptor.Sort(d =>
                    {
                        foreach (var orderByExpression in queryAggregator.OrderByExpressions)
                        {
                            var property = _propertyNameInferrerParser.Parser(orderByExpression.PropertyName) +
                                           orderByExpression.GetKeywordIfNecessary();
                            d.Field(property,
                                orderByExpression.OrderingDirection == OrderingDirection.Asc
                                    ? SortOrder.Ascending
                                    : SortOrder.Descending);
                        }

                        return d;
                    });
                }
                
                if (queryModel.ResultOperators.Any(x => x is GroupResultOperator))
                {
                    var groupResultOperator = (GroupResultOperator) queryModel.ResultOperators.First(x => x is GroupResultOperator);
                    var fieldRaw = groupResultOperator.KeySelector.ToString().Split('.').Last();
                    var field = _propertyNameInferrerParser.Parser(fieldRaw);

                    descriptor.Aggregations(aggs =>
                    {
                        aggs.Terms(field, t => 
                            t.Field(field)); 

                        return aggs;
                    });
                }

                if (queryAggregator.GroupByExpressions.Any())
                {
                    foreach (var groupByExpression in queryAggregator.GroupByExpressions)
                    {
                        var property = _propertyNameInferrerParser.Parser(groupByExpression.PropertyName) +
                                       groupByExpression.GetKeywordIfNecessary();
                        
                        descriptor.Aggregations(a => a
                            .Terms($"group_by_{groupByExpression.PropertyName}", t => 
                                t.Field(property)
                                .Aggregations(aa => aa.TopHits($"data_{groupByExpression.PropertyName}", th => th)))
                        );
                    }
                    
                }
                
                return descriptor;

            });
            
            if (queryModel.SelectClause?.Selector is MemberExpression)
            {
                return JsonConvert.DeserializeObject<IEnumerable<T>>(
                    JsonConvert.SerializeObject(documents.Documents.SelectMany(x => x.Values), Formatting.Indented));
            }
            
            if (queryModel.ResultOperators.Any(x => x is GroupResultOperator))
            {
                return JsonConvert.DeserializeObject<IGrouping<string, T>>(
                    JsonConvert.SerializeObject(documents.Documents, Formatting.Indented));
            }

            if (queryAggregator.GroupByExpressions.Any())
            {
                var groupByExpression = queryAggregator.GroupByExpressions.First();
                
                var groupBy = documents.Aggregations.Terms($"group_by_{groupByExpression.PropertyName}");
                var values = new List<Grouping<T>>();

                var deserializer = new Func<object, T>(input => 
                    JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(input, Formatting.Indented)));

                foreach(var bucket in groupBy.Buckets)
                {
                    //var group = new Grouping<T>(bucket.Key, bucket.TopHits($"data_{groupByExpression.PropertyName}").Documents<object>());
                    var list = bucket.TopHits($"data_{groupByExpression.PropertyName}").Documents<object>();
                    //values.Add(group);
                }
                
                var groupResult = JsonConvert.DeserializeObject<IEnumerable<IGrouping<string, T>>>(
                    JsonConvert.SerializeObject(values, Formatting.Indented));
                return null;
            }

            var result = JsonConvert.DeserializeObject<IEnumerable<T>>(
                JsonConvert.SerializeObject(documents.Documents, Formatting.Indented));

            return result;
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var sequence = ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
        }

        public T ExecuteScalar<T>(QueryModel queryModel)                
        {
            var queryAggregator = _elasticGeneratorQueryModelVisitor.GenerateElasticQuery<T>(queryModel);

            foreach (var resultOperator in queryModel.ResultOperators)
            {
                if (resultOperator is CountResultOperator)
                {
                    var result = _elasticClient.Count<object>(descriptor =>
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

    public class Grouping<T> : IGrouping<string, T>
    {

        private readonly IEnumerable<T> _values;

        public Grouping(string key, IEnumerable<T> values)
        {
            Key = key;
            _values = values;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Key { get; }
    }
}