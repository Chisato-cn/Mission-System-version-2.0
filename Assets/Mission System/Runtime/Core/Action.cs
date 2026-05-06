using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class Action
    {
        public abstract string Content { get; }
        
        public abstract void Execute();
    }
}