using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Mission)]
    public class MCMissionNodeView : MissionChainNode
    {
        public MCMissionNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("output", Direction.Output);
            CreatePort("input", Direction.Input);
        }
    }
}