using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public enum MissionChainType
    {
        Trailblaze,     // 开拓任务
        Finale,         // 终末任务
        Companion,      // 同行任务
        Adventure,      // 冒险任务
        Daily           // 日常任务
    }
    
    [CreateAssetMenu(menuName = "Tomoe/Mission System/Chain")]
    public class MissionChain : ScriptableObject
    {
        [SerializeField] private string guid;
        [SerializeField] private List<Connection> connections = new List<Connection>();
        [SerializeReference] private List<MCNode> nodes = new List<MCNode>();
        
        [SerializeField] private MissionChainType missionChainType;
        [SerializeField, TextArea(1, 10)] private string missionChainName;
        [SerializeField, TextArea(3, 10)] private string missionChainDescription;
        
        public string Guid => guid;
        public string Name => missionChainName;
        public string Description => missionChainDescription;
        public MissionChainType Type => missionChainType;

        public IReadOnlyDictionary<string, MCNode> ReadOnlyNodesDict;
        public IReadOnlyDictionary<string, Connection> ReadOnlyConnectionsDict;
        public IReadOnlyList<Connection> ReadOnlyConnectionsList => connections.AsReadOnly();
        public IReadOnlyList<MCNode> ReadOnlyNodesList => nodes.AsReadOnly();

#if UNITY_EDITOR
        public Vector3 GraphPosition = Vector3.zero;
        public Vector3 GraphScale = Vector3.one;
#endif

        /// <summary>
        /// 通过CreateAssetMenu创建资产调用
        /// </summary>
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();
        }

#if UNITY_EDITOR
        public void RefreshProperty()
        {
            ReadOnlyConnectionsDict = connections.ToDictionary(conn => conn.Guid, conn => conn);
            ReadOnlyNodesDict = nodes.ToDictionary(node => node.Guid, node => node);
        }
        
        public int IndexOfConnection(Connection connection) => connections.IndexOf(connection);
        
        public void AddConnection(Connection connection)
        {
            connections.Add(connection);
            ReadOnlyConnectionsDict = connections.ToDictionary(conn => conn.Guid, conn => conn);
        }
        
        public void RemoveAllConnection(Predicate<Connection> func)
        {
            connections.RemoveAll(func);
            ReadOnlyConnectionsDict = connections.ToDictionary(connection => connection.Guid, connection => connection);
        }
        
        public int IndexOfNode(MCNode node) => nodes.IndexOf(node);
        
        public void AddNode(MCNode nodeData)
        {
            nodes.Add(nodeData);
            ReadOnlyNodesDict = nodes.ToDictionary(node => node.Guid, node => node);
        }
        
        public void RemoveNode(MCNode nodeData)
        {
            nodes.Remove(nodeData);
            ReadOnlyNodesDict = nodes.ToDictionary(node => node.Guid, node => node);
        }
#endif
    }
}