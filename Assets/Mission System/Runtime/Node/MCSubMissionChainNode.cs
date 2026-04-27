using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Sub Mission Chain")]
    public class MCSubMissionChainNode : MCNode
    {
        public MCSubMissionChainNode(MissionChain chain) : base(NodeType.SubMissionChain, chain)
        {
        }
    }

}