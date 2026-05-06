using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.SubMissionChain)]
    public class MCSubMissionChainNodeView : MissionChainNode
    {
        private Mission mission;
        
        public MCSubMissionChainNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("Input", Direction.Input);
            CreatePort("Output", Direction.Output);
            AddToClassList("sub-mission-chain-node");

            mission = ((MCSubMissionChainNode)node).SubMissionChain;
            title = $"【Sub Mission Chain】 {mission.Name}";

            var image = new Image();
            image.image = Resources.Load<Texture2D>("Icon/d_UnityEditor.VersionControl");
            extensionContainer.Add(image);
            
            RefreshExpandedState();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Open Sub Mission Chain Asset", action => MissionChainEditor.Instance.OpenAssetBasedProjectItem(((MCSubMissionChainNode)node).SubChainGuid));
        }

        public override void UpdateView()
        {
            title = $"【Sub Mission Chain】 {mission.Name}";
            RefreshExpandedState();
        }
    }
}