using System.Linq;
using Nest;

namespace LinqToElasticSearch
{
    public class NodeVisitor : INodeVisitor
    {
        public QueryContainer Visit(BoolNode node)
        {
            var queries = node.Children.Select(x => x.Accept(this));
            return new BoolQuery { Should = queries };
        }

        public QueryContainer Visit(OrNode node)
        {
            return new BoolNode(node.Optimize()).Accept(this);
        }

        public QueryContainer Visit(AndNode node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            return new BoolQuery { Must = new[] { left, right } };
        }

        public QueryContainer Visit(NotNode node)
        {
            var child = node.Child.Accept(this);

            return new BoolQuery
            {
                MustNot = new[] { child }
            };
        }

        public QueryContainer Visit(TermNode node)
        {
            return new TermQuery
            {
                Field = node.Field,
                Name = node.Field,
                Value = node.Value
            };
        }

        public QueryContainer Visit(TermsNode node)
        {
            return new TermsQuery
            {
                Field = node.Field,
                Name = node.Field,
                IsVerbatim = true,
                Terms = node.Values
            };
        }

        public QueryContainer Visit(TermsSetNode node)
        {
            return new TermsSetQuery
            {
                Field = node.Field,
                Name = node.Field,
                IsVerbatim = true,
                Terms = node.Values,
                MinimumShouldMatchScript = node.Equal
                    ? new InlineScript($"doc['{node.Field}'].length")
                    : new InlineScript("0")
            };
        }

        public QueryContainer Visit(ExistsNode node)
        {
            return new BoolQuery
            {
                Must = new QueryContainer[]
                {
                    new ExistsQuery
                    {
                        Field = node.Field
                    }
                }
            };
        }

        public QueryContainer Visit(NotExistsNode node)
        {
            return new BoolQuery
            {
                MustNot = new QueryContainer[]
                {
                    new ExistsQuery
                    {
                        Field = node.Field
                    }
                }
            };
        }

        public QueryContainer Visit(DateRangeNode node)
        {
            return new DateRangeQuery
            {
                Field = node.Field,
                Name = node.Field,
                LessThan = node.LessThan,
                LessThanOrEqualTo = node.LessThanOrEqualTo,
                GreaterThan = node.GreaterThan,
                GreaterThanOrEqualTo = node.GreaterThanOrEqualTo
            };
        }

        public QueryContainer Visit(MatchPhraseNode node)
        {
            return new MatchPhraseQuery
            {
                Field = node.Field,
                Name = node.Field,
                Query = (string)node.Value
            };
        }

        public QueryContainer Visit(NumericRangeNode node)
        {
            return new NumericRangeQuery
            {
                Field = node.Field,
                Name = node.Field,
                LessThan = node.LessThan,
                LessThanOrEqualTo = node.LessThanOrEqualTo,
                GreaterThan = node.GreaterThan,
                GreaterThanOrEqualTo = node.GreaterThanOrEqualTo
            };
        }

        public QueryContainer Visit(QueryStringNode node)
        {
            return new QueryStringQuery
            {
                Fields = new[] { node.Field },
                Name = node.Field,
                Query = (string)node.Value
            };
        }

        public QueryContainer Visit(MultiMatchNode node)
        {
            return new MultiMatchQuery
            {
                Fields = new[] { node.Field },
                Name = node.Field,
                Type = TextQueryType.PhrasePrefix,
                Query = (string)node.Value,
                MaxExpansions = 200
            };
        }
    }
}