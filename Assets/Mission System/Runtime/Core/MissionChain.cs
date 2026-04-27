using System.Collections.Generic;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [CreateAssetMenu(menuName = "Tomoe/Mission System/Chain")]
    public class MissionChain : ScriptableObject
    {
        [SerializeField] private string guid;
        [SerializeField] private List<Connection> connections;
        [SerializeReference] private List<MCNode> nodes;
        
        public string Guid => guid;
        public List<MCNode> Nodes => nodes;
        public List<Connection> Connections => connections;

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
    }
}