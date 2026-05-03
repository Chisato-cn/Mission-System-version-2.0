using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Action)]
    public class MCActionNodeView : MissionChainNode
    {
        public MCActionNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("input", Direction.Input);
            AddToClassList("action-node");
            
            title = $"【Action】 {((MCActionNode)node).Title}";
            RefreshExpandedState();
        }

        public override void UpdateView()
        {
            title = $"【Action】 {((MCActionNode)node).Title}";
            RefreshExpandedState();
        }
    }
}