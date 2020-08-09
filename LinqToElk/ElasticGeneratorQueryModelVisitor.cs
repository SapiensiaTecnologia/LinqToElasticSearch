﻿﻿using System;
  using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
  using Remotion.Linq.Clauses.Expressions;
  using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElk
{
    public class ElasticGeneratorQueryModelVisitor: QueryModelVisitorBase
    {
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;
        private QueryAggregator QueryAggregator { get; set; } = new QueryAggregator();

        public ElasticGeneratorQueryModelVisitor(PropertyNameInferrerParser propertyNameInferrerParser)
        {
            _propertyNameInferrerParser = propertyNameInferrerParser;
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
        

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            // var selectorExpression = (MemberExpression) selectClause.Selector;
            // QueryAggregator.PropertiesToSelect.Add(selectorExpression.Member.Name);
            // VisitQueryModel(((QuerySourceReferenceExpression) selectorExpression.Expression));
            base.VisitSelectClause(selectClause, queryModel);;
        }
        
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            if (fromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                VisitQueryModel(subQueryExpression.QueryModel);
            }
            
            base.VisitMainFromClause(fromClause, queryModel);
        }
        
        public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
        {
            var queryContainers = new GeneratorExpressionTreeVisitor(_propertyNameInferrerParser)
                .GetNestExpression(whereClause.Predicate);
            QueryAggregator.QueryContainers.AddRange(queryContainers);
            base.VisitWhereClause (whereClause, queryModel, index);
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
        
        public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            foreach (var ordering in orderByClause.Orderings)
            {
                var memberExpression = (MemberExpression) ordering.Expression;
                var direction = orderByClause.Orderings[0].OrderingDirection;
                var propertyName = memberExpression.Member.Name;
                var type = ((PropertyInfo) memberExpression.Member).PropertyType;
                QueryAggregator.OrderByExpressions.Add(new OrderProperties(propertyName, type, direction)); 
            }
            
            base.VisitOrderByClause (orderByClause, queryModel, index);
        }
    }
}