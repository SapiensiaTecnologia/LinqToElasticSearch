using System.Linq;
using System.Linq.Expressions;
using Nest;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace LinqToElk
{
    public class ElasticQueryable<T> : QueryableBase<T> where T : class
    {
        public ElasticQueryable(ElasticClient elasticClient, string dataId)
            : base(new DefaultQueryProvider(typeof(ElasticQueryable<>), QueryParser.CreateDefault(), new ElasticQueryExecutor<T>(elasticClient, dataId)))
        {
        }

        public ElasticQueryable(IQueryParser queryParser, IQueryExecutor executor)
            : base(new DefaultQueryProvider(typeof(ElasticQueryable<>), queryParser, executor))
        {
        }

        public ElasticQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }
    }
}