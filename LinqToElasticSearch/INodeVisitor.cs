using Nest;

namespace LinqToElasticSearch
{
    public interface INodeVisitor
    {
        QueryContainer Visit(BoolNode node);
        QueryContainer Visit(OrNode node);
        QueryContainer Visit(AndNode node);
        QueryContainer Visit(NotNode node);
        QueryContainer Visit(TermNode node);
        QueryContainer Visit(TermsNode node);
        QueryContainer Visit(TermsSetNode node);
        QueryContainer Visit(ExistsNode node);
        QueryContainer Visit(NotExistsNode node);
        QueryContainer Visit(DateRangeNode node);
        QueryContainer Visit(MatchPhraseNode node);
        QueryContainer Visit(NumericRangeNode node);
        QueryContainer Visit(QueryStringNode node);
        QueryContainer Visit(MultiMatchNode node);
    }
}