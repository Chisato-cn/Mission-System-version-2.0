using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Action = System.Action;
using Tomoe.MissionSystem.Runtime;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainGraphView : GraphView
    {
        private MissionChainSearchTree searchTree;
        
        private MissionChainEditor editor;
        private MissionChain currentChain => editor.MissionChain;
        public Edge TempEdge { get; set; }
        
        public event Action OnWindowFocusChanged; 
        
        public MissionChainGraphView(MissionChainEditor editor)
        {
            Insert(0, new GridBackground());
            styleSheets.Add(Resources.Load<StyleSheet>("StyleSheet/MissionChainGraphView"));
            this.StretchToParentSize();
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.editor = editor;
            graphViewChanged += GraphViewChanged;
            searchTree = ScriptableObject.CreateInstance<MissionChainSearchTree>();
            searchTree.OnEntrySelected += SearchTreeOnEntrySelected;
            nodeCreationRequest += context => 
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchTree);
        }

        public void PopulateGraph()
        {
            DeleteElements(graphElements);
            
            if (currentChain.Nodes.Count == 0)
            {
                var start = CreateNode(typeof(MCStartNode), new Vector2(350, 300));
                var end = CreateNode(typeof(MCEndNode), new Vector2(500, 300));
                var connection = MakeConnection(start.Node, end.Node);
                MakeEdge(start.OutputPort, end.InputPort, connection);
                return;
            }

            foreach (MCNode node in currentChain.Nodes)
            {
                CreateNode(node, node.Position);
            }

            var nodeGraphs = nodes.Select(node => (MissionChainNode)node)
                .ToDictionary(view => view.Node.Guid, view => view);
            foreach (Connection connection in currentChain.Connections)
            {
                var outputNodeView = nodeGraphs[connection.OutputMCNode];
                var inputNodeView = nodeGraphs[connection.InputMCNode];
                MakeEdge(outputNodeView.OutputPort, inputNodeView.InputPort, connection);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            // todo：技术原因，暂时不支持
            // 按照基类添加的顺序逆序移除，避免索引变化
            MissionChainEditorUtility.RemoveMenuItem(evt.menu, "Duplicate");
            MissionChainEditorUtility.RemoveMenuItem(evt.menu, "Paste");  // 注意：Paste 在 Delete 之前
            MissionChainEditorUtility.RemoveMenuItem(evt.menu, "Copy");
            MissionChainEditorUtility.RemoveMenuItem(evt.menu, "Cut");
            MissionChainEditorUtility.RemoveSeparator(evt.menu);
        }
        
        private new GraphViewChange GraphViewChanged(GraphViewChange graphViewChange)
        {
            if (editor.IsChainChangedDueToClearGraph) return graphViewChange;
            
            if (graphViewChange.edgesToCreate is { Count: > 0 })
            {
                Undo.RecordObject(currentChain, "Create Connections");
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var outputNodeView = (MissionChainNode)edge.output.node;
                    var inputNodeView = (MissionChainNode)edge.input.node;

                    var connection = new Connection()
                    {
                        InputMCNode = inputNodeView.Node.Guid,
                        OutputMCNode = outputNodeView.Node.Guid
                    };
                    
                    var connectionView = (ConnectionView)edge;
                    connectionView.Data = connection;
                    connectionView.OnEdgeSelected += element => editor.PopulateInspector(element);
                    
                    outputNodeView.Node.OutputConnections.Add(connection.Guid);
                    inputNodeView.Node.InputConnections.Add(connection.Guid);
                }
            }

            if (graphViewChange.elementsToRemove is { Count: > 0 }) 
            {
                var edgeToDelete = graphViewChange.elementsToRemove.OfType<Edge>().ToHashSet();
                var nodeToDelete = graphViewChange.elementsToRemove.OfType<MissionChainNode>().ToHashSet();
                
                // 过滤不需要处理的边
                var edgeToRemove = new HashSet<Edge>();
                foreach (Edge edge in edgeToDelete)
                {
                    if (nodeToDelete.Contains((MissionChainNode)edge.input.node) ||
                        nodeToDelete.Contains((MissionChainNode)edge.output.node))
                    {
                        edgeToRemove.Add(edge);
                    }
                }
                foreach (Edge edge in edgeToRemove)
                {
                    edgeToDelete.Remove(edge);
                }
                
                Undo.RecordObject(currentChain, "Delete Selections");
                // 删除节点
                foreach (MissionChainNode node in nodeToDelete)
                {
                    HashSet<string> connectionToDelete = new HashSet<string>();
                    foreach (string connectionGuid in node.Node.OutputConnections)
                    {
                        // 记录需要移除的connection的id
                        connectionToDelete.Add(connectionGuid);
                        
                        // 输入节点断开连接
                        var inputMCNodeGuid = currentChain.ReadOnlyConnections[connectionGuid].InputMCNode;
                        var inputNode = currentChain.ReadOnlyNodes[inputMCNodeGuid];
                        inputNode.InputConnections.RemoveAll(conn => conn == connectionGuid);
                    }
                    foreach (string connectionGuid in node.Node.InputConnections)
                    {
                        // 记录需要移除的connection的id
                        connectionToDelete.Add(connectionGuid);
                        
                        var outputMCNodeGuid = currentChain.ReadOnlyConnections[connectionGuid].OutputMCNode;
                        var outputNode = currentChain.ReadOnlyNodes[outputMCNodeGuid];
                        outputNode.OutputConnections.RemoveAll(conn => conn == connectionGuid);
                    }
                
                    node.Node.OutputConnections.Clear();
                    node.Node.InputConnections.Clear();
                    currentChain.Nodes.Remove(node.Node);
                }
                // 删除边
                foreach (Edge edge in edgeToDelete)
                {
                    var inputNode = ((MissionChainNode)edge.input.node).Node;
                    var outputNode = ((MissionChainNode)edge.output.node).Node;

                    var edgeGuid = ((ConnectionView)edge).Data.Guid;
                    inputNode.InputConnections.Remove(edgeGuid);
                    outputNode.OutputConnections.Remove(edgeGuid);
                    currentChain.Connections.RemoveAll(connection => connection.Guid == edgeGuid);
                }
            }
            
            return graphViewChange;
        }

        /// <summary>
        /// 当用户开始拖拽一个端口的连接线时，由GraphView系统自动调用，用来确定当前悬停的端口可以和哪些其他端口建立有效的连接
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(port =>
                    startPort.node != port.node && // 不能自己连接自己
                    startPort.direction != port.direction) // 不能input连input output连output
                .ToList();
            
        }

        private void SearchTreeOnEntrySelected(Type nodeType, Vector2 mouseScreenPosition)
        {
            var position = editor.ChangeCoordinatesToGraphView(mouseScreenPosition);
            Undo.RecordObject(currentChain, "Create Node Data With Connection");
            var nodeView = CreateNode(nodeType, position);

            // 临时edge不存在，无需处理连接
            if (TempEdge == null) return;
            // 如果存在临时edge但edge没有连接任意一个端口，处于游离态，则需要删除
            if (TempEdge.input == null && TempEdge.output == null)
            {
                Debug.LogError("Graph中存在游离态Edge，没有连接任一端口，立即进行删除！");
                RemoveElement(TempEdge);
                TempEdge = null;
                return;
            }
            
            // 判断临时edge的空端口是否和data的端口对应，不对应不予处理
            if (TempEdge.input != null && nodeView.OutputPort != null)
            {
                var linkedNodeView = (MissionChainNode)TempEdge.input.node;
                var connection = MakeConnection(nodeView.Node, linkedNodeView.Node);
                MakeEdge(nodeView.OutputPort, linkedNodeView.InputPort, connection);
            }
            else if (TempEdge.output != null && nodeView.InputPort != null)
            {
                var linkedNodeView = (MissionChainNode)TempEdge.output.node;
                var connection = MakeConnection(linkedNodeView.Node, nodeView.Node);
                MakeEdge(linkedNodeView.OutputPort, nodeView.InputPort, connection);
            }
            
            // 最后无论是否使用，都需要清除临时edge
            TempEdge.input?.Disconnect(TempEdge);
            TempEdge.output?.Disconnect(TempEdge);
            RemoveElement(TempEdge); 
            TempEdge = null;
        }

        private ConnectionView MakeEdge(Port outputPort, Port inputPort, Connection connection)
        {
            ConnectionView edge = new ConnectionView
            {
                output = outputPort,
                input = inputPort,
                Data = connection
            };
            edge.OnEdgeSelected += element => editor.PopulateInspector(element);
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            AddElement(edge);
            return edge;
        }

        private Connection MakeConnection(MCNode outputNode, MCNode inputNode)
        {
            var connection = new Connection()
            {
                InputMCNode = inputNode.Guid,
                OutputMCNode = outputNode.Guid,
            };
            outputNode.OutputConnections.Add(connection.Guid);
            inputNode.InputConnections.Add(connection.Guid);
            currentChain.Connections.Add(connection);
            return connection;
        }

        private MissionChainNode CreateNode(Type nodeType, Vector2 viewPosition) =>
            CreateNode(CreateNodeData(nodeType), viewPosition);
        
        private MissionChainNode CreateNode(MCNode nodeData, Vector2 viewPosition)
        {
            var nodeView = CreateNodeView(nodeData, viewPosition);
            AddElement(nodeView);
            return nodeView;
        }

        private MCNode CreateNodeData(Type nodeType)
        {
            var node= (MCNode)Activator.CreateInstance(nodeType, new object[] { currentChain });
            currentChain.Nodes.Add(node);
            return node;
        }

        private MissionChainNode CreateNodeView(MCNode data, Vector2 viewPosition)
        {
            var types = TypeCache.GetTypesWithAttribute<MCNodeViewAttribute>();
        
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(MissionChainNode))) continue;
            
                if (type.GetCustomAttribute<MCNodeViewAttribute>().DataType == data.Type)
                {
                    var nodeView = (MissionChainNode)Activator.CreateInstance(type, new object[] { data, (Func<EdgeConnectorListener>)NodeViewOnOnCreateEdgeConnector });
                    nodeView.SetPosition(new Rect(viewPosition, nodeView.GetPosition().size)); // 使用默认节点大小
                    nodeView.OnNodeViewSelected += element => editor.PopulateInspector(element);
                    return nodeView;
                }
            }
        
            return null;
        }

        private EdgeConnectorListener NodeViewOnOnCreateEdgeConnector()
        {
            return new EdgeConnectorListener(searchTree,(edge) =>
            {
                TempEdge = new Edge
                {
                    input = edge.input,
                    output = edge.output,
                    candidatePosition = edge.candidatePosition
                };
                TempEdge.input?.Connect(TempEdge);
                TempEdge.output?.Connect(TempEdge);

                OnWindowFocusChanged?.Invoke();
                AddElement(TempEdge);
            });
        }
    }
}