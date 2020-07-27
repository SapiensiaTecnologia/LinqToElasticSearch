using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Parsing.Structure;

namespace LinqToElk
{
    // The item type that our data source will return.
    public class SampleDataSourceItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ElasticQueryable<T> : QueryableBase<T>
    {
        public ElasticQueryable()
            : base(new DefaultQueryProvider(typeof(ElasticQueryable<>), QueryParser.CreateDefault(), new ElasticQueryExecutor()))
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