using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public class MissionChainHandler
    {
        private MissionChain chain;
        private Dictionary<string, MCNode> allActiveMissionNodes;
        private Queue<MCNode> buffer;

        public bool IsCompleted => allActiveMissionNodes.Count == 0;

        public Action<string> OnRemoveParallelMission;
        public Action<string> OnStartSubMissionChain;

        public void Init(MissionChain chain)
        {
            this.chain = chain;
            allActiveMissionNodes = new Dictionary<string, MCNode>();
            buffer = new Queue<MCNode>();

            foreach (var node in chain.ReadOnlyNodesList)
            {
                if (node.Type == NodeType.Start)
                {
                    ExecuteNode(node);
                    break;
                }
            }
        }

        public void UnInit()
        {
            OnRemoveParallelMission = null;
            OnStartSubMissionChain = null;
        }
        
        public void OnMissionsRemoved(List<MissionHandler> handlers, bool isCompleted)
        {
            if (handlers == null || handlers.Count == 0) return;

            // key是node的guid
            var dict = handlers.ToDictionary(handler => handler.Id.Split('|')[1], handler => handler);
            foreach (var handler in handlers)
            {
                if (!allActiveMissionNodes.Remove(handler.Id, out MCNode missionNode)) continue;

                foreach (string connectionGuid in missionNode.OutputConnections)
                {
                    var connection = chain.ReadOnlyConnectionsDict[connectionGuid];
                    if (!connection.IsAvailable) continue;
                    
                    // 移除并行任务的handler
                    if (connection.IsParallelConnection && !dict.ContainsKey(connection.InputMCNode))
                        OnRemoveParallelMission?.Invoke($"{chain.Guid}|{connection.InputMCNode}");
                    
                    // 启用后续节点
                    if (isCompleted && !connection.IsParallelConnection)
                        ExecuteNode(chain.ReadOnlyNodesDict[connection.InputMCNode]);
                }
            }
        }

        public void FlushBuffer(Action<Mission> deployer)
        {
            if (buffer.Count == 0) return;

            while (buffer.Count > 0)
            {
                var node = buffer.Dequeue();

                Mission mission = null;
                if (node is MCMissionNode missionNode) mission = missionNode.Mission;
                else if (node is MCSubMissionChainNode subChainNode) mission = subChainNode.SubMissionChain;
                else
                {
                    Debug.LogWarning($"未知node类型: {node.GetType()}");
                    continue;
                }
                
                allActiveMissionNodes.Add(mission.Id, node);
                deployer?.Invoke(mission);
            }
        }

        public void ExecuteNode(MCNode node)
        {
            if (node == null) return;

            switch (node.Type)
            {
                case NodeType.Start:
                    ExecuteStartNode((MCStartNode)node);
                    break;
                case NodeType.End:
                    ExecuteEndNode((MCEndNode)node);
                    break;
                case NodeType.Action:
                    ExecuteActionNode((MCActionNode)node);
                    break;
                case NodeType.Mission:
                    ExecuteMissionNode((MCMissionNode)node);
                    break;
                case NodeType.SubMissionChain:
                    ExecuteSubMissionChainNode((MCSubMissionChainNode)node);
                    break;
            }
        }

        private void ExecuteMissionNode(MCMissionNode node)
        {
            if (allActiveMissionNodes.ContainsKey(node.Mission.Id)) return;
            
            buffer.Enqueue(node);
            foreach (string connectionGuid in node.OutputConnections)
            {
                var connection = chain.ReadOnlyConnectionsDict[connectionGuid];
                if (!connection.IsAvailable) continue;

                if (connection.IsParallelConnection) ExecuteNode(chain.ReadOnlyNodesDict[connection.InputMCNode]);
            }
        }
        
        private void ExecuteActionNode(MCActionNode node)
        {
            foreach (Action action in node.Actions)
            {
                action.Execute();
            }
        }
        
        private void ExecuteStartNode(MCStartNode node)
        {
            foreach (string connectionGuid in node.OutputConnections)
            {
                var connection = chain.ReadOnlyConnectionsDict[connectionGuid];
                if (!connection.IsAvailable) continue;
                
                var inputNode = chain.ReadOnlyNodesDict[connection.InputMCNode];
                ExecuteNode(inputNode);
            }
        }
        
        private void ExecuteEndNode(MCEndNode node)
        {
            Debug.Log("当前任务连完成");
        }

        private void ExecuteSubMissionChainNode(MCSubMissionChainNode node) 
        {
            OnStartSubMissionChain.Invoke(node.SubChainGuid);
            if (node.OutputConnections.Count > 0) buffer.Enqueue(node);
        }
            
    }
}