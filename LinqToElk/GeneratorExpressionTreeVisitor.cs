using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nest;
using Remotion.Linq.Parsing;

namespace LinqToElk
{
    public class GeneratorExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private IList<QueryContainer> _queryContainers = new List<QueryContainer>();

        public string Property { get; set; }
        public object Value { get; set; }

        public static List<QueryContainer> GetNestExpression(Expression linqExpression)
        {
            var visitor = new GeneratorExpressionTreeVisitor();
            visitor.Visit(linqExpression);
            return visitor.GetNestExpression();
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Visit(expression.Left);
            Visit(expression.Right);
            
            switch (Value)
            {
                case DateTime _:
                    VisitDateProperty(expression.NodeType);
                    break;
                case bool _:
                    VisitBoolProperty(expression.NodeType);
                    break;
                case int _:
                case long _:
                case float _:
                case double _:
                case decimal _:
                    VisitNumericProperty(expression.NodeType);
                    break;
                case string _:
                    VisitStringProperty(expression.NodeType);
                    break;
            }
            
            return expression;
        }

        private void VisitStringProperty(ExpressionType expressionType)
        {
            if (expressionType == ExpressionType.Equal)
            {
                _queryContainers.Add(new MatchPhraseQuery()
                {
                    Field = $"{Property}.keyword",
                    Query = (string) Value
                });
            }
            
            if (expressionType == ExpressionType.NotEqual)
            {
                _queryContainers.Add(new BoolQuery()
                {
                    MustNot =new QueryContainer[]{ new MatchPhraseQuery()
                    {
                        Field = $"{Property}.keyword",
                        Query = (string) Value
                    }}
                } );
            }
        }

        private void VisitNumericProperty(ExpressionType expressionType)
        {
            double.TryParse(Value.ToString(), out var doubleValue);
            switch (expressionType)
            {
                case ExpressionType.GreaterThan:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = Property,
                        GreaterThan = doubleValue
                    });
                    break;
                
                case ExpressionType.GreaterThanOrEqual:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = Property,
                        GreaterThanOrEqualTo = doubleValue
                    });
                    break;
                
                case ExpressionType.LessThan:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = Property,
                        LessThan = doubleValue
                    });
                    break;
                
                case ExpressionType.LessThanOrEqual:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = Property,
                        LessThanOrEqualTo = doubleValue
                    });
                    break;
                
                case ExpressionType.Equal:
                    _queryContainers.Add(new TermQuery()
                    {
                        Field = Property,
                        Value = doubleValue
                    });
                    break;
                
                case ExpressionType.NotEqual:
                    _queryContainers.Add(new BoolQuery()
                    {
                        MustNot =new QueryContainer[]{ new TermQuery()
                        {
                            Field = Property,
                            Value = doubleValue
                        }}
                    } );
                    break;
            }
        }

        private void VisitBoolProperty(ExpressionType expressionNodeType)
        {
            throw new NotImplementedException();
        }

        private void VisitDateProperty(ExpressionType expressionNodeType)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            // In production code, handle this via method lookup tables.
            if (expression.Method.Name ==  "Contains")
            {
                Visit(expression.Object);
                Visit(expression.Arguments[0]);

                
                _queryContainers.Add(new QueryStringQuery()
                {
                    Fields=  new[]{ Property },
                    Query = "*" + Value + "*"
                });
                return expression;
            }
            else
            {
                return base.VisitMethodCall(expression); // throws
            }
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value is string stringValue)
            {
                Value = stringValue;
            }

            if (expression.Value is int intValue)
            {
                Value = intValue;
            }
            
            return expression;
        }
        
        protected override Expression VisitMember(MemberExpression expression)
        {
            Property = expression.Member.Name.ToLower();
            return expression;
        }


        public List<QueryContainer> GetNestExpression()
        {
            return _queryContainers.ToList();
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            // string itemText = FormatUnhandledItem(unhandledItem);
            var itemText = "";
            var message = string.Format("The expression '{0}' (type: {1}) is not supported by this LINQ provider.",
                itemText, typeof(T));
            return new NotSupportedException(message);
        }
    }
}