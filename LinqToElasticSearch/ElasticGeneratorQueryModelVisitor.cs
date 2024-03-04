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

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            switch (selectClause.Selector)
            {
                case NewExpression newExpression:
                    var i = 0;
                    foreach (var arg in newExpression.Arguments)
                    {
                        string fieldName = null;
                        string propertyName = null;
                        Type propertyType = null;
                        ProjectionType projectionType = ProjectionType.Property;
                        
                        switch (arg)
                        {
                            case MemberExpression memberExpression:
                                fieldName = memberExpression.Member.Name;
                                propertyName = newExpression.Members[i].Name;
                                propertyType = memberExpression.Type;
                                projectionType = ProjectionType.Property;
                                break;
                            
                            case SubQueryExpression subQueryExpression:
                                fieldName = null;
                                propertyName = newExpression.Members[i].Name;
                                propertyType = subQueryExpression.Type;

                                if (subQueryExpression.QueryModel.ResultOperators.Any(x => x is CountResultOperator))
                                {
                                    projectionType = ProjectionType.Count;
                                }
                                else if (subQueryExpression.QueryModel.ResultOperators.Any(x => x is MaxResultOperator))
                                {
                                    projectionType = ProjectionType.Max;
                                }
                                else if (subQueryExpression.QueryModel.ResultOperators.Any(x => x is MinResultOperator))
                                {
                                    projectionType = ProjectionType.Min;
                                }

                                break;
                        }

                        QueryAggregator.SelectExpressions.Add(new SelectProperties(fieldName, propertyName, propertyType, i, projectionType));
                        i++;
                    }
                    
                    break;
                    
            }

            base.VisitSelectClause(selectClause, queryModel);
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
                    var propertiesToGroup = new List<GroupByProperties>();
                    
                    switch (groupResultOperator.KeySelector)
                    {
                        case MemberExpression memberExpression:
                            propertiesToGroup.Add(new GroupByProperties(memberExpression.Member.Name, memberExpression.Member.Name, memberExpression.Type));
                            break;
                        
                        case NewExpression newExpression:

                            var i = 0;
                            foreach (var arg in newExpression.Arguments)
                            {
                                var memberExpression = (MemberExpression) arg;

                                var fieldName = memberExpression.Member.Name;
                                var propertyName = newExpression.Members[i].Name;
                                var propertyType = memberExpression.Type;
                                
                                propertiesToGroup.Add(new GroupByProperties(fieldName, propertyName, propertyType));
                                i++;
                            }
                            break;
                    }
                    
                    QueryAggregator.GroupByExpressions.AddRange(propertiesToGroup);
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
                var elasticFieldName = DiscoveryElasticFieldName(propertyName);
                QueryAggregator.OrderByExpressions.Add(new OrderProperties(elasticFieldName, type, direction)); 
            }
            
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        private string DiscoveryElasticFieldName(string propertyName)
        {
            if (QueryAggregator.GroupByExpressions.Any())
            {
                if (QueryAggregator.GroupByExpressions.Count == 1)
                {
                    return QueryAggregator.GroupByExpressions[0].ElasticFieldName;
                }
                
                return QueryAggregator.GroupByExpressions.First(x => x.PropertyName == propertyName).ElasticFieldName;
            }

            return propertyName;
        }
    }
}