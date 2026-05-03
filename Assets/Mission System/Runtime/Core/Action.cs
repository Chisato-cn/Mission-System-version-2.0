using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class Action
    {
        public abstract void Execute();
    }
}