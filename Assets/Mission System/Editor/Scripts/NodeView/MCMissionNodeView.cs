using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Mission)]
    public class MCMissionNodeView : MissionChainNode
    {
        private Mission mission;
        
        public MCMissionNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("output", Direction.Output);
            CreatePort("input", Direction.Input);
            AddToClassList("mission-node");

            mission = ((MCMissionNode)node).Mission;
            
            UpdateView();
        }

        public void UpdateView()
        {
            title = $"【Mission】 {mission.Name}";
            
            
        }
    }
}