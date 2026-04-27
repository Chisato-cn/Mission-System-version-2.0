using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Mission")]
    public class MCMissionNode : MCNode
    {
        [SerializeField] private Mission mission;
        
        public MCMissionNode(MissionChain chain) : base(NodeType.Mission, chain)
        {
            mission = new Mission($"{chain.Guid}|{Guid}");
        }
    }
}