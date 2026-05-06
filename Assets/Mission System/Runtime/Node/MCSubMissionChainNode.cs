using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable, MCNode("Sub Mission Chain")]
    public class MCSubMissionChainNode : MCNode
    {
        [SerializeField] private string subChainGuid;
        
#if UNITY_EDITOR
        [CustomPropertyDrawerType("Tomoe.MissionSystem.Editor.MissionDrawer")]
#endif
        [SerializeField] private Mission subChainMission;

        public string SubChainGuid => subChainGuid;
        public Mission SubMissionChain => subChainMission;
        
        public MCSubMissionChainNode(MissionChain chain) : base(NodeType.SubMissionChain, chain)
        {
            subChainMission = new Mission($"{chain.Guid}|{Guid}");
        }
    }
}