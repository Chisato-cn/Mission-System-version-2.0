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
        [SerializeField, HideInInspector] private string guid;
        [SerializeField] private List<Connection> connections;
        [SerializeReference] private List<MCNode> nodes;
        
        [SerializeField] private MissionChainType missionChainType;
        [SerializeField, TextArea(1, 10)] private string missionChainName;
        [SerializeField, TextArea(3, 10)] private string missionChainDescription;
        
        public string Guid => guid;
        public string Name => missionChainName;
        public string Description => missionChainDescription;
        public MissionChainType Type => missionChainType;
        public IReadOnlyDictionary<string, MCNode> ReadOnlyNodes;
        public IReadOnlyDictionary<string, Connection> ReadOnlyConnections;

#if UNITY_EDITOR
        [HideInInspector] public Vector3 GraphPosition = Vector3.zero;
        [HideInInspector] public Vector3 GraphScale = Vector3.one;
        public List<Connection> Connections => connections;
        public List<MCNode> Nodes => nodes;
#endif

        /// <summary>
        /// 通过CreateAssetMenu创建资产调用
        /// </summary>
        private void OnEnable()
        {
            nodes = new List<MCNode>();
            connections = new List<Connection>();
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();
        }

        private void OnValidate()
        {
            ReadOnlyNodes = nodes.ToDictionary(node => node.Guid, node => node);
            ReadOnlyConnections = connections.ToDictionary(connection => connection.Guid, connection => connection);
        }
    }
}