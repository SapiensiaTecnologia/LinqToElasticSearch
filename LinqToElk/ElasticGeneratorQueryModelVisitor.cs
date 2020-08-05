using System.Collections.Generic;
using Nest;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        private List<QueryContainer> _queryContainers = new List<QueryContainer>();

        public static IList<QueryContainer> GenerateElasticQuery(QueryModel queryModel)
        {
            var visitor = new ElasticGeneratorQueryModelVisitor ();
            visitor.VisitQueryModel (queryModel);
            return visitor._queryContainers;
        } 
        
        public override void VisitQueryModel (QueryModel queryModel)
        {
            queryModel.SelectClause.Accept (this, queryModel);
            queryModel.MainFromClause.Accept (this, queryModel);
            VisitBodyClauses (queryModel.BodyClauses, queryModel);
            VisitResultOperators (queryModel.ResultOperators, queryModel);
        }

        public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
        {
            // var queryContainers = GeneratorExpressionTreeVisitor.GetNestExpression(selectClause.Selector);
            // _queryContainers.AddRange(queryContainers);
            base.VisitSelectClause (selectClause, queryModel);
        }
        
        public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
        {
            var queryContainers = GeneratorExpressionTreeVisitor.GetNestExpression(whereClause.Predicate);
            _queryContainers.AddRange(queryContainers);
            base.VisitWhereClause (whereClause, queryModel, index);
        }
        
        public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            // var queryContainers = orderByClause.Orderings.SelectMany(o => GeneratorExpressionTreeVisitor.GetNestExpression((o.Expression)));
            // _queryContainers.AddRange(queryContainers);
            base.VisitOrderByClause (orderByClause, queryModel, index);
        }
    }
}