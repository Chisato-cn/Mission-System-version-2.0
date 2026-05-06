using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    [Flags]
    public enum ComparisionType
    {
        Equal = 1,
        NotEqual = 2,
        LessThan = 4,
        GreaterThan = 8,
    }
    
    [Serializable]
    public class AmountComparisonCondition : Condition
    {
        [SerializeField] private string targetGuid;
        [SerializeField] private int targetAmount;
        [SerializeField] private ComparisionType comparisionType;

        public override bool IsConditionMet => Compare();

        private bool Compare()
        {
            var amount = GameAPI.Get(targetGuid).Amount;
            switch (comparisionType)
            {
                case ComparisionType.Equal:
                    return amount == targetAmount;
                case ComparisionType.NotEqual:
                    return amount != targetAmount;
                case ComparisionType.LessThan:
                    return amount < targetAmount;
                case ComparisionType.GreaterThan:
                    return amount > targetAmount;
                case ComparisionType.Equal | ComparisionType.GreaterThan:
                    return amount >= targetAmount;
                case ComparisionType.Equal | ComparisionType.LessThan:
                    return amount <= targetAmount;
            }
            return false;
        }
    }
}