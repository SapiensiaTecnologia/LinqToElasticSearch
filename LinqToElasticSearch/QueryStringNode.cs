using Nest;

namespace LinqToElasticSearch
{
    public class QueryStringNode : Node
    {
        public string Field { get; set; }
        public object Value { get; set; }

        public QueryStringNode(string field, object value)
        {
            Field = field;
            Value = value;
        }

        public override QueryContainer Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}