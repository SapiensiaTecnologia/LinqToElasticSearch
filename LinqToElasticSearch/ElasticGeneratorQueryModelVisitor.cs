using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Nest;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace LinqToElasticSearch
{
    public class ElasticGeneratorQueryModelVisitor<TU> : QueryModelVisitorBase
    {
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;
        private readonly INodeVisitor _nodeVisitor;
        private QueryAggregator QueryAggregator { get; set; } = new QueryAggregator();

        public ElasticGeneratorQueryModelVisitor(PropertyNameInferrerParser propertyNameInferrerParser)
        {
            _propertyNameInferrerParser = propertyNameInferrerParser;
            _nodeVisitor = new NodeVisitor();
        }

        public QueryAggregator GenerateElasticQuery<T>(QueryModel queryModel)
        {
            QueryAggregator = new QueryAggregator();
            VisitQueryModel(queryModel);
            return QueryAggregator;
        } 
        
        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);
        }
        
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            if (fromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                VisitQueryModel(subQueryExpression.QueryModel);
            }
            
            base.VisitMainFromClause(fromClause, queryModel);
        }
        
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var tree = new GeneratorExpressionTreeVisitor<TU>(_propertyNameInferrerParser);
            tree.Visit(whereClause.Predicate);
            if (QueryAggregator.Query == null)
            {
                var node = tree.QueryMap[whereClause.Predicate];
                QueryAggregator.Query = node.Accept(_nodeVisitor);
            }
            else
            {
                var left = QueryAggregator.Query;
                var right = tree.QueryMap[whereClause.Predicate].Accept(_nodeVisitor);

                var query = new BoolQuery
                {
                    Must = new[] { left, right }
                };

                QueryAggregator.Query = query;
            }
            base.VisitWhereClause(whereClause, queryModel, index);
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

                if (resultOperator is GroupResultOperator groupResultOperator)
                {
                    var members = new List<Tuple<string, Type, bool>>();
                    
                    switch (groupResultOperator.KeySelector)
                    {
                        case MemberExpression memberExpression:
                            var isKeyword = memberExpression.Member.CustomAttributes.Any(x => x.AttributeType == typeof(KeywordAttribute));
                            members.Add(new Tuple<string, Type, bool>(memberExpression.Member.Name, memberExpression.Type, isKeyword));
                            break;
                        case NewExpression newExpression:
                            members.AddRange(newExpression.Arguments
                                .Cast<MemberExpression>()
                                .Select(memberExpression => new Tuple<string, Type, bool>(memberExpression.Member.Name, memberExpression.Type, false)));
                            break;
                    }
                    
                    members.ForEach(property =>
                    {
                        QueryAggregator.GroupByExpressions.Add(new GroupByProperties(property.Item1, property.Item2, property.Item3));
                    });
                }
            }
            
            base.VisitResultOperators(resultOperators, queryModel);
        }
        
        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            foreach (var ordering in orderByClause.Orderings)
            {
                var memberExpression = (MemberExpression) ordering.Expression;
                var direction = orderByClause.Orderings[0].OrderingDirection;
                var propertyName = memberExpression.Member.Name;
                var type = memberExpression.Type;
                var isKeyword = memberExpression.Member.CustomAttributes.Any(x => x.AttributeType == typeof(KeywordAttribute));
                QueryAggregator.OrderByExpressions.Add(new OrderProperties(propertyName, type, direction, isKeyword)); 
            }
            
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }
    }
}