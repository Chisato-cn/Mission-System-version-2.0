using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    [Serializable]
    public class SubMissionChainRequirement : MissionRequirement
    {
        [SerializeField] private string subMissionChainGuid;
        
        protected override Type handlerType => typeof(SubMissionChainRequirementHandler);
        public string SubMissionChainGuid => subMissionChainGuid;
        
        public override bool CheckMessage(object message)
        {
            if (message is not GameMessage gameMessage) return false;
            return gameMessage.Type == GameMessage.GameMessageType.SubMissionChainCompleted;
        }
    }

    public class SubMissionChainRequirementHandler : MissionRequirementHandler
    {
        public SubMissionChainRequirementHandler(MissionRequirement requirement) : base(requirement)
        {
        }

        protected override bool UseMessage(object message)
        {
            if (message is not GameMessage gameMessage || gameMessage.HasUsed) return false;

            string subMissionChainGuid = gameMessage.Args as string;
            if (string.IsNullOrEmpty(subMissionChainGuid) || subMissionChainGuid != ((SubMissionChainRequirement)requirement).SubMissionChainGuid) return false;
            
            // gameMessage.Use();       一般不需要
            return true;
        }
    }
}