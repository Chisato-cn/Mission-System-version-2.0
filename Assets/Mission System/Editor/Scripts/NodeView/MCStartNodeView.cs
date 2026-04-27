using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Start)]
    public class MCStartNodeView : MissionChainNode
    {
        public MCStartNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("output", Direction.Output);
        }
    }
}