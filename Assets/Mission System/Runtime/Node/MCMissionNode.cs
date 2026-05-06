using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Mission")]
    public class MCMissionNode : MCNode
    {
#if UNITY_EDITOR
        [CustomPropertyDrawerType("Tomoe.MissionSystem.Editor.MissionDrawer")]
#endif
        [SerializeField] private Mission mission;

        public Mission Mission => mission;

        public MCMissionNode(MissionChain chain) : base(NodeType.Mission, chain)
        {
            mission = new Mission($"{chain.Guid}|{Guid}");
        }
    }
}