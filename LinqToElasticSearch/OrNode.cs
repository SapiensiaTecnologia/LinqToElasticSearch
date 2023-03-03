using System.Collections.Generic;
using Nest;

namespace LinqToElasticSearch
{
    public class OrNode : Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }

        private readonly IList<Node> _optimizedNodes = new List<Node>();

        public override QueryContainer Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public IList<Node> Optimize()
        {
            _optimizedNodes.Clear();
            DoOptimize(this);
            return _optimizedNodes;
        }

        private void DoOptimize(Node node)
        {
            if (node is OrNode or)
            {
                if (or.Left is OrNode l && !(or.Right is OrNode))
                {
                    if (!(l.Left is OrNode))
                    {
                        _optimizedNodes.Add(l.Left);
                    }
                    else
                    {
                        DoOptimize(l.Left);
                    }

                    if (!(l.Right is OrNode))
                    {
                        _optimizedNodes.Add(l.Right);
                    }
                    else
                    {
                        DoOptimize(l.Right);
                    }

                    _optimizedNodes.Add(or.Right);
                }
                else if (!(or.Left is OrNode) && or.Right is OrNode r)
                {
                    _optimizedNodes.Add(or.Left);

                    if (!(r.Left is OrNode))
                    {
                        _optimizedNodes.Add(r.Left);
                    }
                    else
                    {
                        DoOptimize(r.Left);
                    }

                    if (!(r.Right is OrNode))
                    {
                        _optimizedNodes.Add(r.Right);
                    }
                    else
                    {
                        DoOptimize(r.Right);
                    }
                }
                else if (or.Left is OrNode left && or.Right is OrNode right)
                {
                    if (!(left.Left is OrNode))
                    {
                        _optimizedNodes.Add(left.Left);
                    }
                    else
                    {
                        DoOptimize(left.Left);
                    }

                    if (!(left.Right is OrNode))
                    {
                        _optimizedNodes.Add(left.Right);
                    }
                    else
                    {
                        DoOptimize(left.Right);
                    }

                    if (!(right.Left is OrNode))
                    {
                        _optimizedNodes.Add(right.Left);
                    }
                    else
                    {
                        DoOptimize(right.Left);
                    }

                    if (!(right.Right is OrNode))
                    {
                        _optimizedNodes.Add(right.Right);
                    }
                    else
                    {
                        DoOptimize(right.Right);
                    }
                }
                else
                {
                    _optimizedNodes.Add(or.Left);
                    _optimizedNodes.Add(or.Right);
                }
            }
        }
    }
}