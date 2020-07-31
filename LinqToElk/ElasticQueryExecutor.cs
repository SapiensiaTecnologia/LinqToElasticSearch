using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Remotion.Linq;

namespace LinqToElk
{
    public class ElasticQueryExecutor<TU> : IQueryExecutor where TU : class
    {
        private readonly ElasticClient _elasticClient;
        private readonly string _dataId;

        public ElasticQueryExecutor(ElasticClient elasticClient, string dataId)
        {
            _elasticClient = elasticClient;
            _dataId = dataId;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var queryContainers = ElasticGeneratorQueryModelVisitor.GenerateElasticQuery(queryModel);
            
            var documents= _elasticClient.Search<TU>(descriptor =>
            {
                descriptor.Index(_dataId);
                
                if (queryContainers.Any())
                {
                    descriptor.Query(q => q.Bool(x => x.Must(queryContainers.ToArray())));
                }
                else
                {
                    descriptor.MatchAll();
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
            throw new NotImplementedException();
        }
    }
}