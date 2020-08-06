﻿﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        public QueryAggregator QueryAggregator { get; set; } = new QueryAggregator();
        public static QueryAggregator GenerateElasticQuery(QueryModel queryModel)
        {
            var visitor = new ElasticGeneratorQueryModelVisitor ();
            visitor.VisitQueryModel(queryModel);
            return visitor.QueryAggregator;
        } 
        
        public override void VisitQueryModel (QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators,
            QueryModel queryModel)
        {
            foreach (var resultOperator in resultOperators)
            {
                if (resultOperator is SkipResultOperator skipResultOperator)
                {
                    QueryAggregator.Skip = skipResultOperator.GetConstantCount();
                }
                
                if (resultOperator is TakeResultOperator takeResultOperator)
                {
                    QueryAggregator.Take = takeResultOperator.GetConstantCount();
                }
            }
            base.VisitResultOperators(resultOperators, queryModel);
        }
        
        public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
        {
            // _queryParts.AddFromPart (fromClause);

            base.VisitMainFromClause (fromClause, queryModel);
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
            QueryAggregator.QueryContainers.AddRange(queryContainers);
            base.VisitWhereClause (whereClause, queryModel, index);
        }
        
        public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            var queryContainers = orderByClause.Orderings.SelectMany(o => GeneratorExpressionTreeVisitor.GetNestExpression((o.Expression)));
            QueryAggregator.QueryContainers.AddRange(queryContainers);
            
            if (orderByClause.Orderings[0].Expression is MemberExpression  memberExpression)
            {
                
                var direction = orderByClause.Orderings[0].OrderingDirection;
                var property = memberExpression.Member.Name;
                QueryAggregator.OrderBy = new OrderProperties(property, direction);

            }
            base.VisitOrderByClause (orderByClause, queryModel, index);
        }
    }
}