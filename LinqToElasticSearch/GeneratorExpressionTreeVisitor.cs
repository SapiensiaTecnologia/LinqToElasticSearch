using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Elasticsearch.Net;
using Nest;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace LinqToElasticSearch
{
    public class GeneratorExpressionTreeVisitor<T> : ThrowingExpressionVisitor
    {
        private readonly PropertyNameInferrerParser _propertyNameInferrerParser;

        private object Value { get; set; }
        private string PropertyName { get; set; }
        private Type PropertyType { get; set; }
        private bool Not { get; set; }

        public IDictionary<Expression, QueryContainer> QueryMap { get; } =
            new Dictionary<Expression, QueryContainer>();

        public GeneratorExpressionTreeVisitor(PropertyNameInferrerParser propertyNameInferrerParser)
        {
            _propertyNameInferrerParser = propertyNameInferrerParser;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                Not = true;
                Value = false;
            }

            Visit(expression.Operand);

            if (QueryMap.TryGetValue(expression.Operand, out var query))
            {
                QueryMap[expression] = query;
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.Left is BinaryExpression && expression.Right is ConstantExpression)
            {
                HandleBinarySide(expression, expression.Left);
                return expression;
            }

            if (expression.Right is BinaryExpression && expression.Left is ConstantExpression)
            {
                HandleBinarySide(expression, expression.Right);
                return expression;
            }

            Visit(expression.Left);
            Visit(expression.Right);

            HandleExpression(expression);

            if (expression.NodeType == ExpressionType.OrElse || expression.NodeType == ExpressionType.AndAlso)
            {
                var left = QueryMap[expression.Left];
                var right = QueryMap[expression.Right];

                var query = new BoolQuery
                {
                    Should = expression.NodeType == ExpressionType.OrElse ? new[] { left, right } : null,
                    Must = expression.NodeType == ExpressionType.AndAlso ? new[] { left, right } : null
                };

                QueryMap[expression] = ParseQuery(query);
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Value = expression.Value;
            HandleExpression(expression);
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "ToLower":
                    Visit(expression.Object);
                    break;
                case "Contains":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    HandleContains(expression);
                    break;
                case "StartsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    HandleStartsWith(expression);
                    break;
                case "EndsWith":
                    Visit(expression.Object);
                    Visit(expression.Arguments[0]);
                    HandleEndsWith(expression);
                    break;
                default:
                    return base.VisitMethodCall(expression); // throws
            }

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            Visit(expression.Expression);

            PropertyType = Nullable.GetUnderlyingType(expression.Type) ?? expression.Type;
            PropertyName = _propertyNameInferrerParser.Parser(expression.Member.Name);

            // Implicit boolean is only a member visit
            if (expression.Type == typeof(bool))
            {
                if (!(Value is bool))
                {
                    // Implicit boolean is always true
                    Value = true;
                }

                QueryMap[expression] = HandleBoolProperty(expression);
            }

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            foreach (var resultOperator in expression.QueryModel.ResultOperators)
            {
                QueryContainer query = null;

                switch (resultOperator)
                {
                    case ContainsResultOperator containsResultOperator:
                        Visit(containsResultOperator.Item);
                        Visit(expression.QueryModel.MainFromClause.FromExpression);

                        if (containsResultOperator.Item.Type == typeof(Guid))
                        {
                            query = new TermsQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                IsVerbatim = true,
                                Terms = ((IEnumerable<Guid>)Value).Select(x => x.ToString())
                            };
                        }

                        if (containsResultOperator.Item.Type == typeof(Guid?))
                        {
                            query = new TermsQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                IsVerbatim = true,
                                Terms = ((IEnumerable<Guid?>)Value).Select(x => x.ToString())
                            };
                        }

                        QueryMap[expression] = ParseQuery(query);
                        break;
                    case AnyResultOperator anyResultOperator:
                        var whereClauses = expression.QueryModel.BodyClauses.OfType<WhereClause>().ToList();

                        foreach (var whereClause in whereClauses)
                        {
                            Visit(whereClause.Predicate);
                        }

                        Visit(expression.QueryModel.MainFromClause.FromExpression);

                        query = new TermsQuery
                        {
                            Field = PropertyName,
                            Name = PropertyName,
                            IsVerbatim = true,
                            Terms = new[] { Value }
                        };

                        QueryMap[expression] = ParseQuery(query);
                        break;
                    case AllResultOperator allResultOperator:
                        Visit(allResultOperator.Predicate);
                        Visit(expression.QueryModel.MainFromClause.FromExpression);

                        query = new TermsSetQuery
                        {
                            Field = PropertyName,
                            Name = PropertyName,
                            IsVerbatim = true,
                            Terms = new[] { Value },
                            MinimumShouldMatchScript = new InlineScript($"doc['{PropertyName}'].length")
                        };

                        if (allResultOperator.Predicate.NodeType == ExpressionType.Equal)
                        {
                            query = ParseQuery(query);
                        }
                        else if (allResultOperator.Predicate.NodeType == ExpressionType.NotEqual)
                        {
                            Not = true;
                            query = ParseQuery(query);
                        }
                        else
                        {
                            return base.VisitSubQuery(expression);
                        }

                        QueryMap[expression] = query;
                        break;
                    default:
                        return base.VisitSubQuery(expression); // throws
                }
            }

            return expression;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            return expression;
        }

        protected override Exception CreateUnhandledItemException<T2>(T2 unhandledItem, string visitMethod)
        {
            var itemText = "";

            var message = string.Format(
                "The expression '{0}' (type: {1}) is not supported by this LINQ provider.",
                itemText,
                typeof(T)
            );

            return new NotSupportedException(message);
        }

        private QueryContainer ParseQuery(QueryContainer query)
        {
            if (query == null)
            {
                return null;
            }

            if (Not)
            {
                Not = false;

                return new BoolQuery
                {
                    MustNot = new[] { query }
                };
            }

            return query;
        }

        private QueryContainer HandleNullProperty(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                case ExpressionType.Equal:
                    return new BoolQuery
                    {
                        MustNot = new QueryContainer[]
                        {
                            new ExistsQuery
                            {
                                Field = PropertyName
                            }
                        }
                    };
                case ExpressionType.NotEqual:
                    return new BoolQuery
                    {
                        Must = new QueryContainer[]
                        {
                            new ExistsQuery
                            {
                                Field = PropertyName
                            }
                        }
                    };
                default:
                    return (BoolQuery)null;
            }
        }

        private QueryContainer HandleEnumProperty(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return new TermQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        Value = ConvertEnumValue(typeof(T), PropertyName, Value)
                    };
                case ExpressionType.NotEqual:
                    return new BoolQuery
                    {
                        MustNot = new QueryContainer[]
                        {
                            new TermQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                Value = ConvertEnumValue(typeof(T), PropertyName, Value)
                            }
                        }
                    };
                default:
                    return null;
            }
        }

        private QueryContainer HandleDateProperty(Expression expression)
        {
            if (!(Value is DateTime dateTime))
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.GreaterThan:
                    return new DateRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        GreaterThan = dateTime
                    };
                case ExpressionType.GreaterThanOrEqual:
                    return new DateRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        GreaterThanOrEqualTo = dateTime
                    };
                case ExpressionType.LessThan:
                    return new DateRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        LessThan = dateTime
                    };
                case ExpressionType.LessThanOrEqual:
                    return new DateRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        LessThanOrEqualTo = dateTime
                    };
                case ExpressionType.Equal:
                    return new DateRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        GreaterThanOrEqualTo = dateTime,
                        LessThanOrEqualTo = dateTime
                    };
                case ExpressionType.NotEqual:
                    return new BoolQuery
                    {
                        MustNot = new QueryContainer[]
                        {
                            new DateRangeQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                GreaterThanOrEqualTo = dateTime,
                                LessThanOrEqualTo = dateTime
                            }
                        }
                    };
                default:
                    return null;
            }
        }

        private QueryContainer HandleBoolProperty(Expression expression)
        {
            if (!(Value is bool boolValue))
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess:
                case ExpressionType.Equal:
                    return new TermQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        Value = boolValue
                    };
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                    return new TermQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        Value = !boolValue
                    };
                default:
                    return (TermQuery)null;
            }
        }

        private QueryContainer HandleStringProperty(Expression expression)
        {
            if (Value is Guid guid)
            {
                Value = guid.ToString();
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return new MatchPhraseQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        Query = (string)Value
                    };
                case ExpressionType.NotEqual:
                    return new BoolQuery
                    {
                        MustNot = new QueryContainer[]
                        {
                            new MatchPhraseQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                Query = (string)Value
                            }
                        }
                    };
                default:
                    return null;
            }
        }

        private QueryContainer HandleNumericProperty(Expression expression)
        {
            var value = Value;
            
            if (Value is TimeSpan timeSpan)
            {
                value = timeSpan.Ticks;
            }
            
            double.TryParse(value.ToString(), out var doubleValue);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return new TermQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        Value = doubleValue
                    };
                case ExpressionType.NotEqual:
                    return new BoolQuery
                    {
                        MustNot = new QueryContainer[]
                        {
                            new TermQuery
                            {
                                Field = PropertyName,
                                Name = PropertyName,
                                Value = doubleValue
                            }
                        }
                    };
                case ExpressionType.GreaterThan:
                    return new NumericRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        GreaterThan = doubleValue
                    };
                case ExpressionType.GreaterThanOrEqual:
                    return new NumericRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        GreaterThanOrEqualTo = doubleValue
                    };
                case ExpressionType.LessThan:
                    return new NumericRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        LessThan = doubleValue
                    };
                case ExpressionType.LessThanOrEqual:
                    return new NumericRangeQuery
                    {
                        Field = PropertyName,
                        Name = PropertyName,
                        LessThanOrEqualTo = doubleValue
                    };
                default:
                    return null;
            }
        }

        private void HandleBinarySide(BinaryExpression binaryExpression, Expression sideExpression)
        {
            if (binaryExpression.Left == sideExpression)
            {
                Visit(binaryExpression.Right);
            }

            if (binaryExpression.Right == sideExpression)
            {
                Visit(binaryExpression.Left);
            }

            var anotherSideValue = (bool)Value;

            Visit(sideExpression);

            if (QueryMap.TryGetValue(sideExpression, out var query))
            {
                if (anotherSideValue == false)
                {
                    Not = true;
                    query = ParseQuery(query);
                    QueryMap[sideExpression] = query;
                }

                QueryMap[binaryExpression] = query;
            }
        }

        private void HandleExpression(Expression expression)
        {
            QueryContainer query;

            if (Value == null)
            {
                query = HandleNullProperty(expression);
            }
            else if (PropertyType != null && PropertyType.IsEnum)
            {
                query = HandleEnumProperty(expression);
            }
            else
            {
                switch (Value)
                {
                    case DateTime _:
                        query = HandleDateProperty(expression);
                        break;
                    case bool _:
                        query = HandleBoolProperty(expression);
                        break;
                    case int _:
                    case long _:
                    case float _:
                    case double _:
                    case decimal _:
                    case TimeSpan _:
                        query = HandleNumericProperty(expression);
                        break;
                    case string _:
                    case Guid _:
                        query = HandleStringProperty(expression);
                        break;
                    default:
                        query = null;
                        break;
                }
            }

            if (query != null)
            {
                QueryMap[expression] = ParseQuery(query);
            }
        }

        private void HandleContains(Expression expression)
        {
            var tokens = ((string)Value).Split(' ');

            QueryContainer query;

            if (tokens.Length == 1)
            {
                query = new QueryStringQuery
                {
                    Fields = new[] { PropertyName },
                    Name = PropertyName,
                    Query = "*" + Value + "*"
                };
            }
            else
            {
                query = new MultiMatchQuery
                {
                    Fields = new[] { PropertyName },
                    Name = PropertyName,
                    Type = TextQueryType.PhrasePrefix,
                    Query = (string)Value,
                    MaxExpansions = 200
                };
            }

            QueryMap[expression] = ParseQuery(query);
        }

        private void HandleStartsWith(Expression expression)
        {
            var query = new QueryStringQuery
            {
                Fields = new[] { PropertyName },
                Name = PropertyName,
                Query = Value + "*"
            };

            QueryMap[expression] = ParseQuery(query);
        }

        private void HandleEndsWith(Expression expression)
        {
            var query = new QueryStringQuery
            {
                Fields = new[] { PropertyName },
                Name = PropertyName,
                Query = "*" + Value
            };

            QueryMap[expression] = ParseQuery(query);
        }

        private object ConvertEnumValue(Type entityType, string propertyName, object value)
        {
            var enumValue = Enum.Parse(PropertyType, value.ToString());
            var prop = entityType.GetProperties().FirstOrDefault(x => x.Name.ToLower() == propertyName.ToLower());

            if (prop != null && prop.GetCustomAttributes(true)
                    .Any(attribute => attribute is StringEnumAttribute && prop.PropertyType.IsEnum))
            {
                return enumValue.ToString();
            }

            return (int)enumValue;
        }
    }
}