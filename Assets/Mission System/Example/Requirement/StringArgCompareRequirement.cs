using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    [Serializable]
    public class StringArgCompareRequirement : MissionRequirement
    {
        [SerializeField] private string stringArgs;
        
        protected override Type handlerType => typeof(StringArgCompareRequirementHandler);
        public  string StringArgs => stringArgs;
        
        public override bool CheckMessage(object message)
        {
            if (message is not GameMessage gameMessage) return false;
            return gameMessage.Type == GameMessage.GameMessageType.StringArgsCompare;
        }
    }
    
    public class StringArgCompareRequirementHandler : MissionRequirementHandler
    {
        public StringArgCompareRequirementHandler(MissionRequirement requirement) : base(requirement)
        {
        }

        protected override bool UseMessage(object message)
        {
            if (message is not GameMessage gameMessage || gameMessage.HasUsed) return false;

            string args = gameMessage.Args as string;
            if (string.IsNullOrEmpty(args) || args != ((StringArgCompareRequirement)requirement).StringArgs) return false;
            
            // gameMessage.Use();       一般不需要
            return true;
        }
    }
}