﻿﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
  using System.Reflection;
  using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        private GeneratorExpressionTreeVisitor _generatorExpressionTreeVisitor;
        private QueryAggregator QueryAggregator { get; set; } = new QueryAggregator();

        public ElasticGeneratorQueryModelVisitor(PropertyNameInferrerParser propertyNameInferrerParser)
        {
            _generatorExpressionTreeVisitor = new GeneratorExpressionTreeVisitor(propertyNameInferrerParser);
        }

        public QueryAggregator GenerateElasticQuery(QueryModel queryModel)
        {
            QueryAggregator = new QueryAggregator();
            VisitQueryModel(queryModel);
            return QueryAggregator;
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
        
        public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
        {
            var queryContainers = _generatorExpressionTreeVisitor.GetNestExpression(whereClause.Predicate);
            QueryAggregator.QueryContainers.AddRange(queryContainers);
            base.VisitWhereClause (whereClause, queryModel, index);
        }
        
        public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            if (orderByClause.Orderings[0].Expression is MemberExpression  memberExpression)
            {
                
                var direction = orderByClause.Orderings[0].OrderingDirection;
                var propertyName = memberExpression.Member.Name;
                var type = ((PropertyInfo) memberExpression.Member).PropertyType;
                QueryAggregator.OrderBy = new OrderProperties(propertyName, type, direction);

            }
            base.VisitOrderByClause (orderByClause, queryModel, index);
        }
    }
}