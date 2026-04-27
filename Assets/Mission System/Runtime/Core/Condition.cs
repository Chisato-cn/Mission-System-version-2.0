using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class Condition
    {
        public abstract bool IsConditionMet { get; }
    }
}