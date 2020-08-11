using System.Linq;
using System.Linq.Expressions;
using Nest;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace LinqToElasticSearch
{
    public class ElasticQueryable<T> : QueryableBase<T>
    {
        public ElasticQueryable(IElasticClient elasticClient, string dataId)
            : base(new DefaultQueryProvider(typeof(ElasticQueryable<>), QueryParser.CreateDefault(), new ElasticQueryExecutor(elasticClient, dataId)))
        {
        }

        public ElasticQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }
    }
}