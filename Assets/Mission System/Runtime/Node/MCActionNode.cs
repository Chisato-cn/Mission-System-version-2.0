using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Action")]
    public class MCActionNode : MCNode
    {
#if UNITY_EDITOR
        [CustomPropertyDrawerType("Tomoe.MissionSystem.Editor.SerializedReferenceListPropertyDrawer")]
#endif
        [SerializeReference, HideInInspector] private Action[] actions;
        
        public IReadOnlyList<Action> Actions => actions;
            
        public MCActionNode(MissionChain chain) : base(NodeType.Action, chain)
        {
            
        }
    }
}