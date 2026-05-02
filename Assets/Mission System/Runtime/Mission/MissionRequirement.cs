using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class MissionRequirement
    {
#if UNITY_EDITOR
        [CustomPropertyDrawerType("ConditionDrawer")]
#endif
        [SerializeReference] protected Condition[] Conditions;
        
        protected abstract Type handlerType { get; }
        public abstract string Description { get; }
        
        public abstract bool CheckMessage(object message);

        public MissionRequirementHandler CreateHandler() =>
            (MissionRequirementHandler)Activator.CreateInstance(handlerType, this);
    }
}