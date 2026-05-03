using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Action")]
    public class MCActionNode : MCNode
    {
        [SerializeField] private string title;
        [SerializeReference, HideInInspector] private Action[] actions;

        public string Title => title;
        public Action[] Actions => actions;
            
        public MCActionNode(MissionChain chain) : base(NodeType.Action, chain)
        {
            
        }
    }
}