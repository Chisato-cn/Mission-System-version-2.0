using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("End")]
    public class MCEndNode : MCNode
    {
        public MCEndNode(MissionChain chain) : base(NodeType.End, chain)
        {
        }
    }
}