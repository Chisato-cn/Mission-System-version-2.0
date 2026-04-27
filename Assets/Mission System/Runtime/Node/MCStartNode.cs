using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public class MCStartNode : MCNode
    {
        public MCStartNode(MissionChain chain) : base(NodeType.Start, chain)
        {
        }
    }
}