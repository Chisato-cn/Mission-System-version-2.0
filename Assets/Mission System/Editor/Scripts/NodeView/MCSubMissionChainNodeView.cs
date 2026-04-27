using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.SubMissionChain)]
    public class MCSubMissionChainNodeView : MissionChainNode
    {
        public MCSubMissionChainNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("Input", Direction.Input);
        }
    }
}