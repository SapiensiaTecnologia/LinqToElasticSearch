using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToElk.Extensions;
using Nest;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace LinqToElk
{
    public class GeneratorExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private IList<QueryContainer> _queryContainers = new List<QueryContainer>();

        public string Property { get; set; }
        public object Value { get; set; }
        public ExpressionType? NodeType { get; set; }

        public static List<QueryContainer> GetNestExpression(Expression linqExpression)
        {
            var visitor = new GeneratorExpressionTreeVisitor();
            visitor.Visit(linqExpression);
            return visitor.GetNestExpression();
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            NodeType = expression.NodeType;

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
                case ExpressionType.Equal:
                    _queryContainers.Add(new TermQuery()
                    {
                        Field = Property,
                        Value = doubleValue
                    });
                    // _queryContainers.Add(new MatchQuery()
                    // {
                    //     Field = Property,
                    //     Query = doubleValue.ToString()
                    // });
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
            }
        }
        private void VisitEnumProperty(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    _queryContainers.Add(new MatchQuery()
                    {
                        Field = Property,
                        Query = Value.ToString()
                    });
                    break;
            }
        }

        private void VisitBoolProperty(ExpressionType expressionNodeType)
        {
            if (Value is bool boolValue)
                switch (expressionNodeType)
                {
                    case ExpressionType.Equal:
                        _queryContainers.Add(new TermQuery()
                        {
                            Field = Property,
                            Value = boolValue
                        });
                        break;
                    case ExpressionType.NotEqual:
                    case ExpressionType.Not:
                        _queryContainers.Add(new TermQuery()
                        {
                            Field = Property,
                            Value = !boolValue
                        }); 
                        break;
                }
        }

        private void VisitDateProperty(ExpressionType expressionNodeType)
        {

            if (Value is DateTime dateTime)
                switch (expressionNodeType)
                {
                    case ExpressionType.GreaterThan:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = Property,
                            GreaterThan = dateTime
                        });
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = Property,
                            GreaterThanOrEqualTo = dateTime
                        });
                        break;
                    case ExpressionType.LessThan:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = Property,
                            LessThan = dateTime
                        });
                        break;
                    case ExpressionType.LessThanOrEqual:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = Property,
                            LessThanOrEqualTo = dateTime
                        });
                        break;
                    case ExpressionType.Equal:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = Property,
                            GreaterThanOrEqualTo = dateTime,
                            LessThanOrEqualTo = dateTime 
                        });
                        break;
                    case ExpressionType.NotEqual:
                        _queryContainers.Add(new BoolQuery()
                        {
                            MustNot =new QueryContainer[]{ new DateRangeQuery()
                            {
                                Field = Property,
                                GreaterThanOrEqualTo = dateTime,
                                LessThanOrEqualTo = dateTime 
                            }}
                        } );
                        break;
                } 
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                // In production code, handle this via method lookup tables.
                case "ToLower":
                    // if (Value is string value)
                    //     Value = value.ToLower();
                
                    return expression;
                case "Contains":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    _queryContainers.Add(new QueryStringQuery()
                    {
                        Fields=  new[]{ Property },
                        Query = "*" + Value + "*"
                    });
                    return expression;
                case "StartsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    _queryContainers.Add(new QueryStringQuery()
                    {
                        Fields=  new[]{ Property },
                        Query = Value + "*"
                    });
                    return expression;
                case "EndsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    _queryContainers.Add(new QueryStringQuery()
                    {
                        Fields=  new[]{ Property },
                        Query = "*" + Value
                    });
                    return expression;
                default:
                    return base.VisitMethodCall(expression); // throws
            }
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    Value = false;
                    break;
            }
            Visit(expression.Operand);
            
            // VisitBoolProperty(expression.NodeType);
        
            return expression;
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
            
            if (expression.Value is DateTime dateTime)
            {
                Value = dateTime;
            }
            
            if (expression.Value is bool boolValue)
            {
                Value = boolValue;
            }
            
            return expression;
        }
        
        protected override Expression VisitMember(MemberExpression expression)
        {
            Property = expression.Member.Name.ToLowerFirstChar();

            if (expression.Type == typeof(bool))
            {
                if (!(Value is bool))
                {
                    Value = true;
                }

                if (NodeType == null)
                {
                    VisitBoolProperty(ExpressionType.Equal);
                }
            }
            
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