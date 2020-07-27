using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        private IList<BinaryExpression> _queryWhereParts = new List<BinaryExpression>();

        public static IList<BinaryExpression> GenerateElasticQuery(QueryModel queryModel)
        {
            var visitor = new ElasticGeneratorQueryModelVisitor ();
            visitor.VisitQueryModel (queryModel);
            return visitor._queryWhereParts;
        } 
        
        public override void VisitQueryModel (QueryModel queryModel)
        {
            queryModel.SelectClause.Accept (this, queryModel);
            queryModel.MainFromClause.Accept (this, queryModel);
            VisitBodyClauses (queryModel.BodyClauses, queryModel);
            // VisitResultOperators (queryModel.ResultOperators, queryModel);
        }

        public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (whereClause.Predicate is BinaryExpression expression)
            {
                _queryWhereParts.Add(expression);
            }

            base.VisitWhereClause (whereClause, queryModel, index);
        }
    }
}