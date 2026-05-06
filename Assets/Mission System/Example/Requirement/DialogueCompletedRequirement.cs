using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    [Serializable]
    public class DialogueCompletedRequirement : MissionRequirement
    {
        [SerializeField] private string dialogueGuid;
        
        protected override Type handlerType => typeof(DialogueCompletedRequirementHandler);
        public string DialogueGuid => dialogueGuid;
        
        public override bool CheckMessage(object message)
        {
            if (message is not GameMessage gameMessage) return false;
            return gameMessage.Type == GameMessage.GameMessageType.DialogueCompleted;
        }
    }
    
    public class DialogueCompletedRequirementHandler : MissionRequirementHandler
    {
        public DialogueCompletedRequirementHandler(MissionRequirement requirement) : base(requirement)
        {
        }

        protected override bool UseMessage(object message)
        {
            if (message is not GameMessage gameMessage || gameMessage.HasUsed) return false;

            string dialogueGuid = gameMessage.Args as string;
            if (string.IsNullOrEmpty(dialogueGuid) || dialogueGuid != ((DialogueCompletedRequirement)requirement).DialogueGuid) return false;
            
            // gameMessage.Use();       一般不需要
            return true;
        }
    }
}