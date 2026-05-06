using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class MissionRequirement
    {
        [SerializeField] protected string description;        // todo：没有序列化出来
#if UNITY_EDITOR
        [CustomPropertyDrawerType("Tomoe.MissionSystem.Editor.SerializedReferenceListPropertyDrawer")]
#endif
        [SerializeReference] protected Condition[] conditions;
        
        
        protected abstract Type handlerType { get; }
        public string Description => description;
        
        public abstract bool CheckMessage(object message);

        public MissionRequirementHandler CreateHandler() =>
            (MissionRequirementHandler)Activator.CreateInstance(handlerType, this);
    }
}