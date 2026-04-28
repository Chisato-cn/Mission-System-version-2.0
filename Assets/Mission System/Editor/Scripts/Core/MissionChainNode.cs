using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class ConnectionView : Edge
    {
        public Connection Data { get; set; }

        public event Action<GraphElement> OnEdgeSelected; 

        public override void OnSelected()
        {
            base.OnSelected();
            OnEdgeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnEdgeSelected?.Invoke(null);
        }
    }
    
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        private MissionChainSearchTree searchTree;
        private Action<Edge> onMakeTempEdge;
        
        public EdgeConnectorListener(MissionChainSearchTree searchTree, Action<Edge> onMakeTempEdge)
        {
            this.searchTree = searchTree;
            this.onMakeTempEdge = onMakeTempEdge;
        }

        public void OnDrop(GraphView graphView, Edge edge) { }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // 需要深拷贝Edge，否则在异步完成之前Edge就被销毁了
            onMakeTempEdge?.Invoke(edge);
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position)), searchTree);
        }
    }
    
    public abstract class MissionChainNode : Node
    {
        protected MCNode node;
        
        public MCNode Node => node;
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }
        
        private Func<EdgeConnectorListener> onCreateEdgeConnector;
        public event Action<GraphElement> OnNodeViewSelected; 
        
        protected MissionChainNode(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StyleSheet/MissionChainNode"));
            
            this.node = node;
            this.onCreateEdgeConnector = onCreateEdgeConnector;
        }

        protected Port CreatePort(string portName, Direction direction, bool multiConnected = true)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                direction,
                multiConnected ? Port.Capacity.Multi : Port.Capacity.Single,
                typeof(bool));

            if (direction == Direction.Input)
            {
                port.name = "input-port";
                InputPort = port;
                inputContainer.Add(port);
            }
            else
            {
                port.name = "output-port";
                OutputPort = port;
                outputContainer.Add(port);
            }
            
            port.AddManipulator(new EdgeConnector<ConnectionView>(onCreateEdgeConnector.Invoke()));
            port.portName = portName;
            RefreshPorts();
            return port;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeViewSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeViewSelected?.Invoke(null);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            node.Position = newPos.position;
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) 
            => Port.Create<ConnectionView>(orientation, direction, capacity, type);
    }
}