using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class MissionRequirement
    {
        protected abstract Type handlerType { get; }
        
        public abstract bool CheckMessage(object message);

        public MissionRequirementHandler CreateHandler() =>
            (MissionRequirementHandler)Activator.CreateInstance(handlerType, this);
    }
}