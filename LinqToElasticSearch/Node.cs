using Nest;

namespace LinqToElasticSearch
{
    public abstract class Node
    {
        public abstract QueryContainer Accept(INodeVisitor visitor);
    }
}