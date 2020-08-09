﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
  using System.Reflection;
  using Nest;
  using Remotion.Linq.Clauses.Expressions;
  using Remotion.Linq.Parsing;

namespace LinqToElk
{
    public class GeneratorExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private List<QueryContainer> _queryContainers = new List<QueryContainer>();
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;
        private bool Not { get; set; }
        private string PropertyName { get; set; }
        private object Value { get; set; }
        private ExpressionType? NodeType { get; set; }
        public Type PropertyType { get; set; }


        public GeneratorExpressionTreeVisitor(PropertyNameInferrerParser propertyNameInferrerParser)
        {
            _propertyNameInferrerParser = propertyNameInferrerParser;
        }

        public List<QueryContainer> GetNestExpression(Expression linqExpression)
        {
            Visit(linqExpression);
            
            var qc = (new BoolQuery()
            {
                Should = ShouldList
            }); 
            
            _queryContainers.Add(qc);
            return _queryContainers.ToList();
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            NodeType = expression.NodeType;

            Visit(expression.Left);
            Visit(expression.Right);

            if (Value == null)
            {
                if (expression.NodeType == ExpressionType.Equal)
                {
                    _queryContainers.Add(new BoolQuery()
                    {
                        MustNot = new QueryContainer[]
                        {
                            new ExistsQuery()
                            {
                                Field = PropertyName
                            }
                        }
                    });
                }
                if (expression.NodeType == ExpressionType.NotEqual)
                {
                    _queryContainers.Add(new BoolQuery()
                    {
                        Must = new QueryContainer[]
                        {
                            new ExistsQuery()
                            {
                                Field = PropertyName
                            }
                        }
                    });
                }
            }

            if (Value is Enum)
            {
                Value = (int) Value;
            }
            
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
                case Guid _:
                    VisitStringProperty(expression.NodeType);
                    break;
            }
            
            return expression;
        }


        private void VisitStringProperty(ExpressionType expressionType)
        {
            if (Value is Guid valueGuid)
            {
                Value = valueGuid.ToString();
            }
            
            
            if (expressionType == ExpressionType.Equal)
            {
                _queryContainers.Add(new MatchPhraseQuery()
                {
                    Field = $"{PropertyName}",
                    Query = (string) Value
                });
            }
            
            if (expressionType == ExpressionType.NotEqual)
            {
                _queryContainers.Add(new BoolQuery()
                {
                    MustNot =new QueryContainer[]{ new MatchPhraseQuery()
                    {
                        Field = $"{PropertyName}",
                        Query = (string) Value
                    }}
                } );
            }

            if (expressionType == ExpressionType.OrElse)
            {
                ShouldList.AddRange(_queryContainers);
                _queryContainers.Clear();
            }
        }

        public List<QueryContainer> ShouldList = new List<QueryContainer>();

        private void VisitNumericProperty(ExpressionType expressionType)
        {
            double.TryParse(Value.ToString(), out var doubleValue);
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    _queryContainers.Add(new TermQuery()
                    {
                        Field = PropertyName,
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
                            Field = PropertyName,
                            Value = doubleValue
                        }}
                    } );
                    break;
                case ExpressionType.GreaterThan:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = PropertyName,
                        GreaterThan = doubleValue
                    });
                    break;
                
                case ExpressionType.GreaterThanOrEqual:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = PropertyName,
                        GreaterThanOrEqualTo = doubleValue
                    });
                    break;
                
                case ExpressionType.LessThan:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = PropertyName,
                        LessThan = doubleValue
                    });
                    break;
                
                case ExpressionType.LessThanOrEqual:
                    _queryContainers.Add(new NumericRangeQuery()
                    {
                        Field = PropertyName,
                        LessThanOrEqualTo = doubleValue
                    });
                    break;
                case ExpressionType.OrElse:
                    var qc = (new BoolQuery()
                    {
                        Should = new[]{ _queryContainers[0], _queryContainers[1]}
                    }); 
                    _queryContainers.Clear();
                    _queryContainers.Add(qc);
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
                        Field = PropertyName,
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
                            Field = PropertyName,
                            Value = boolValue
                        });
                        break;
                    case ExpressionType.NotEqual:
                    case ExpressionType.Not:
                        _queryContainers.Add(new TermQuery()
                        {
                            Field = PropertyName,
                            Value = !boolValue
                        }); 
                        break;
                    case ExpressionType.OrElse:
                        var qc = (new BoolQuery()
                        {
                            Should = new[]{ _queryContainers[0], _queryContainers[1]}
                        }); 
                        _queryContainers.Clear();
                        _queryContainers.Add(qc);
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
                            Field = PropertyName,
                            GreaterThan = dateTime
                        });
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = PropertyName,
                            GreaterThanOrEqualTo = dateTime
                        });
                        break;
                    case ExpressionType.LessThan:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = PropertyName,
                            LessThan = dateTime
                        });
                        break;
                    case ExpressionType.LessThanOrEqual:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = PropertyName,
                            LessThanOrEqualTo = dateTime
                        });
                        break;
                    case ExpressionType.Equal:
                        _queryContainers.Add(new DateRangeQuery()
                        {
                            Field = PropertyName,
                            GreaterThanOrEqualTo = dateTime,
                            LessThanOrEqualTo = dateTime 
                        });
                        break;
                    case ExpressionType.NotEqual:
                        _queryContainers.Add(new BoolQuery()
                        {
                            MustNot =new QueryContainer[]{ new DateRangeQuery()
                            {
                                Field = PropertyName,
                                GreaterThanOrEqualTo = dateTime,
                                LessThanOrEqualTo = dateTime 
                            }}
                        });
                        break;
                    case ExpressionType.OrElse:
                        var qc = (new BoolQuery()
                        {
                            Should = new[]{ _queryContainers[0], _queryContainers[1]}
                        }); 
                        _queryContainers.Clear();
                        _queryContainers.Add(qc);
                        break;
                } 
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            // In production code, handle this via method lookup tables.
            QueryStringQuery query;
            switch (expression.Method.Name)
            {
                case "ToLower":
                    Visit(expression.Object);
                    break;
                case "Contains":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    query = (new QueryStringQuery()
                    {
                        Fields=  new[]{ PropertyName },
                        Query = "*" + Value + "*"
                    });
                    AddQueryContainer(query);
                    break;
                case "StartsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    query = (new QueryStringQuery()
                    {
                        Fields=  new[]{ PropertyName },
                        Query = Value + "*"
                    });
                    AddQueryContainer(query);
                    break;
                case "EndsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    query = (new QueryStringQuery()
                    {
                        Fields=  new[]{ PropertyName },
                        Query = "*" + Value
                    });
                    AddQueryContainer(query);
                    break;
                default:
                    return base.VisitMethodCall(expression); // throws
            }
            
            return expression;
        }

        private void AddQueryContainer(QueryContainer query)
        {
            if (query != null) 
                if (Not)
                {
                    _queryContainers.Add(new BoolQuery()
                    {
                        MustNot = new[]{query}
                    });
                    Not = false;
                }
                else
                {
                    _queryContainers.Add(query);
                }
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    Not = true;
                    Value = false;
                    break;
            }
            Visit(expression.Operand);
            
            return expression;
        }


        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Value = expression.Value;
            return expression;
        }
        
        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            return expression;
        }
        
        protected override Expression VisitMember(MemberExpression expression)
        {
            Visit(expression.Expression);
            PropertyName = _propertyNameInferrerParser.Parser(expression.Member.Name);
            PropertyType = expression.Type;

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