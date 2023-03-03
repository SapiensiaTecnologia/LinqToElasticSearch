using Nest;

namespace LinqToElasticSearch
{
    public class NumericRangeNode : Node
    {
        public string Field { get; set; }
        public double? GreaterThan { get; set; }
        public double? GreaterThanOrEqualTo { get; set; }
        public double? LessThan { get; set; }
        public double? LessThanOrEqualTo { get; set; }

        public NumericRangeNode(string field)
        {
            Field = field;
        }

        public override QueryContainer Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}