using System.Collections.Generic;
using System.Linq.Expressions;
using Nest;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        private IList<QueryContainer> _queryContainers = new List<QueryContainer>();

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
            // VisitResultOperators (queryModel.ResultOperators, queryModel);
        }

        public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
        {
            _queryContainers = GeneratorExpressionTreeVisitor.GetNestExpression(whereClause.Predicate);
            
            // if (whereClause.Predicate is BinaryExpression expression)
            // {
            //     _queryWhereParts.Add(expression);
            //
            //     _queryContainers.Add(new MatchPhraseQuery()
            //     {
            //         Field = $"{((MemberExpression) expression.Left).Member.Name}.keyword",
            //         Query = (string) ((ConstantExpression) expression.Right).Value
            //     });
            // };

            
            
            
            
            // queryModel.TransformExpressions(e =>
            //     ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(e, mapping, true));

            // ExpressionTreeVisitor
            
            base.VisitWhereClause (whereClause, queryModel, index);
        }
    }
}