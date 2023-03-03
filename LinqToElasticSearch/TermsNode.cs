using System.Collections.Generic;
using Nest;

namespace LinqToElasticSearch
{
    public class TermsNode : Node
    {
        public string Field { get; set; }
        public IEnumerable<object> Values { get; set; }

        public TermsNode(string field, IEnumerable<string> values)
        {
            Field = field;
            Values = values;
        }

        public override QueryContainer Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}