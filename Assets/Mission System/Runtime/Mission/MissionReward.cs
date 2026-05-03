using System;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public abstract class MissionReward
    {
        public string IconPath { get; }
        public string ItemName { get; }
        public int ItemAmount { get; }
        
        public abstract void ApplyReward();
    }
}