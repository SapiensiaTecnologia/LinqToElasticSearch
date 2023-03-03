using Nest;

namespace LinqToElasticSearch
{
    public class ExistsNode : Node
    {
        public string Field { get; set; }

        public ExistsNode(string field)
        {
            Field = field;
        }

        public override QueryContainer Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}