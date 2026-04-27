using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class MissionReward
    {
        public abstract void ApplyReward();
    }
}