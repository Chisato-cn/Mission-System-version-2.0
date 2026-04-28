using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public enum NodeType
    {
        Start,
        End,
        
        Action,
        Mission,
        SubMissionChain,
    }
    
    [Serializable]
    public abstract class MCNode
    {
        [SerializeField, HideInInspector] private string guid;
        [SerializeField] private NodeType type;
        [SerializeField] private List<string> outputConnections;
        
#if UNITY_EDITOR
        public Vector2 Position;
        public List<string> InputConnections;
#endif
        
        public NodeType Type => type;
        public string Guid => guid;
        public List<string> OutputConnections => outputConnections;
        
        protected MCNode(NodeType type, MissionChain chain)
        {
            this.type = type;
            guid = System.Guid.NewGuid().ToString();
            outputConnections = new List<string>();
#if UNITY_EDITOR
            InputConnections = new List<string>();
#endif
        }
    }
}