using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Action)]
    public class MCActionNodeView : MissionChainNode
    {
        public MCActionNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("input", Direction.Input);
            AddToClassList("action-node");
            
            title = "【Action】";
            UpdateView();
        }

        public override void UpdateView()
        {
            extensionContainer.Clear();
            
            var actions = ((MCActionNode)node).Actions;
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    var item = new MissionExtensionItemStyle_IconPlusContent(Resources.Load<Texture2D>("Icon/cs Script Icon"), action.Content);
                    extensionContainer.Add(item);
                }
            }
            
            RefreshExpandedState();
        }
    }
}