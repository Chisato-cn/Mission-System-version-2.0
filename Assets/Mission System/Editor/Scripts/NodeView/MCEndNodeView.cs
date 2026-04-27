using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.End)]
    public class MCEndNodeView : MissionChainNode
    {
        public MCEndNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("input", Direction.Input);
        }
    }
}