using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    public class ItemCollectedMessageArg
    {
        public readonly string ItemGuid;
        public readonly int ItemAmount;
        
        public ItemCollectedMessageArg(string itemGuid, int itemAmount)
        {
            ItemGuid = itemGuid;
            ItemAmount = itemAmount;
        }
    }
    
    [Serializable]
    public class ItemCollectedRequirement : MissionRequirement
    {
        [SerializeField] private string itemGuid;
        [SerializeField] private int targetAmount;
        
        protected override Type handlerType => typeof(ItemCollectedRequirementHandler);
        public string ItemGuid => itemGuid;
        public int TargetAmount => targetAmount;
        
        public override bool CheckMessage(object message)
        {
            if (message is not GameMessage gameMessage) return false;
            return gameMessage.Type == GameMessage.GameMessageType.ItemCollected;
        }
    }
    
    public class ItemCollectedRequirementHandler : MissionRequirementHandler
    {
        private ItemCollectedRequirement itemCollectedRequirement;
        private int currentAmount;
        
        public ItemCollectedRequirementHandler(MissionRequirement requirement) : base(requirement)
        {
            itemCollectedRequirement = (ItemCollectedRequirement)requirement;
            currentAmount = 0;
        }

        protected override bool UseMessage(object message)
        {
            if (message is not GameMessage gameMessage || gameMessage.HasUsed) return false;
            
            ItemCollectedMessageArg args = gameMessage.Args as ItemCollectedMessageArg;
            if (args == null || args.ItemGuid != itemCollectedRequirement.ItemGuid) return false;
            
            currentAmount += args.ItemAmount == 0 ? 1 : args.ItemAmount;        // 如果参数的数量为0，默认+1
            return currentAmount >= itemCollectedRequirement.TargetAmount;
        }
    }
}