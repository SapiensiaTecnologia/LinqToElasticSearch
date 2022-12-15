using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToElasticSearch.Extensions;
using Nest;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElasticSearch
{
    public class ElasticQueryExecutor<TK>: IQueryExecutor
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _dataId;
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;
        private readonly ElasticGeneratorQueryModelVisitor<TK> _elasticGeneratorQueryModelVisitor;
        private const int ElasticQueryLimit = 10000;
            
        public ElasticQueryExecutor(IElasticClient elasticClient, string dataId)
        {
            _elasticClient = elasticClient;
            _dataId = dataId;
            _propertyNameInferrerParser = new PropertyNameInferrerParser(_elasticClient);
            _elasticGeneratorQueryModelVisitor = new ElasticGeneratorQueryModelVisitor<TK>(_propertyNameInferrerParser);
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var queryAggregator = _elasticGeneratorQueryModelVisitor.GenerateElasticQuery<T>(queryModel);
           var documents = AsyncHelper.RunSync(() => ExecuteAsync(queryModel, queryAggregator));
            
            if (queryModel.SelectClause?.Selector is MemberExpression)
            {
                return JsonConvert.DeserializeObject<IEnumerable<T>>(
                    JsonConvert.SerializeObject(documents.Documents.SelectMany(x => x.Values), Formatting.Indented));
            }

            if (queryAggregator.GroupByExpressions.Any())
            {
                var docDeserializer = new Func<object, TK>(input => 
                    JsonConvert.DeserializeObject<TK>(JsonConvert.SerializeObject(input, Formatting.Indented)));

                var originalGroupingType = queryModel.GetResultType().GenericTypeArguments.First();
                var originalGroupingGenerics = originalGroupingType.GetGenericArguments();
                var originalKeyGenerics = originalGroupingGenerics.First();

                var genericListType = typeof(List<>).MakeGenericType(originalGroupingType);
                var values = (IList)Activator.CreateInstance(genericListType);
                
                var composite = documents.Aggregations.Composite("composite");
            
                foreach(var bucket in composite.Buckets)
                {
                    var key = GenerateKey(bucket.Key, originalKeyGenerics);
                    var list = bucket.TopHits("data_composite").Documents<object>().Select(docDeserializer).ToList();

                    var grouping = typeof(Grouping<,>);
                    var groupingGenerics = grouping.MakeGenericType(originalGroupingGenerics);
                    var groupingInstance = Activator.CreateInstance(groupingGenerics, key, list);
                    values.Add(groupingInstance);
                }
                
                return values.Cast<T>();
            }

            var result = JsonConvert.DeserializeObject<IEnumerable<T>>(
                JsonConvert.SerializeObject(documents.Documents, Formatting.Indented));

            return result;
        }

        private async Task<ISearchResponse<IDictionary<string, object>>> ExecuteAsync(QueryModel queryModel,
            QueryAggregator queryAggregator)
        {
            return
                await _elasticClient.SearchAsync<IDictionary<string, object>>(descriptor =>
                {
                    descriptor.Index(_dataId);

                    if (queryModel.SelectClause != null &&
                        queryModel.SelectClause.Selector is MemberExpression memberExpression)
                    {
                        descriptor.Source(x => x.Includes(f =>
                            f.Field(_propertyNameInferrerParser.Parser(memberExpression.Member.Name))));
                    }

                    if (queryAggregator.Skip != null)
                    {
                        descriptor.From(queryAggregator.Skip);
                    }
                    else
                    {
                        if (queryModel.ResultOperators.Any(x =>
                                x is FirstResultOperator || x is SingleResultOperator))
                        {
                            descriptor.Size(1);
                        }
                        else
                        {
                            descriptor.Size(ElasticQueryLimit);
                        }
                    }

                    if (queryAggregator.Take != null)
                    {
                        var take = queryAggregator.Take.Value;
                        var skip = queryAggregator.Skip ?? 0;

                        if (skip + take > ElasticQueryLimit)
                        {
                            var exceedCount = skip + take - ElasticQueryLimit;
                            take -= exceedCount;
                        }

                        descriptor.Take(take);
                        descriptor.Size(take);
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

                    if (queryAggregator.GroupByExpressions.Any())
                    {
                        descriptor.Aggregations(a =>
                        {

                            a.Composite("composite", c =>
                                c.Sources(so =>
                                    {
                                        queryAggregator.GroupByExpressions.ForEach(gbe =>
                                        {
                                            var property = _propertyNameInferrerParser.Parser(gbe.PropertyName) +
                                                           gbe.GetKeywordIfNecessary();
                                            so.Terms($"group_by_{gbe.PropertyName}", t => t.Field(property));
                                        });

                                        return so;
                                    })
                                    .Aggregations(aa => aa
                                        .TopHits("data_composite", th => th)
                                    )
                            );

                            return a;
                        });

                    }

                    return descriptor;

                });
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

                    if (result > ElasticQueryLimit)
                    {
                        result = ElasticQueryLimit;
                    }
                    
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return (T)converter.ConvertFromString(result.ToString());
                    }
                }
            }
            
            return default(T);
        }

        private dynamic GenerateKey(CompositeKey ck, Type keyGenerics)
        {
            if (keyGenerics.IsConstructedGenericType == false)
            {
                if (keyGenerics == typeof(DateTime))
                {            
                    var date = (long) ck.Values.First();
                    return FormatDateTimeKey(date);
                }
                return ck.Values.First();
            }
            
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var entry in ck)
            {
                var key = entry.Key.Replace("group_by_", "");
                var type = keyGenerics.GetProperties().FirstOrDefault(x => x.Name == key)?.PropertyType;
                
                if (type != null && type == typeof(DateTime))
                {            
                    var date = (long) entry.Value;
                    expando[key] = FormatDateTimeKey(date);
                    continue;
                }
                
                expando[key] = entry.Value;
            }

            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expando), keyGenerics);
        }

        private static DateTime FormatDateTimeKey(long d)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return date.AddMilliseconds(d).ToLocalTime();
        }
    }

    public class Grouping<TKey, TElem> : IGrouping<TKey, TElem>
    {
        public TKey Key { get; }
        
        private readonly IEnumerable<TElem> _values;

        public Grouping(TKey key, IEnumerable<TElem> values)
        {
            Key = key;
            _values = values;
        }

        public IEnumerator<TElem> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}