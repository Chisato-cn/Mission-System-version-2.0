using System;
using System.Collections.Generic;

namespace Tomoe.MissionSystem.Runtime
{
    public class MissionSystem
    {
        private Dictionary<string, MissionHandler> missionHandlers = new Dictionary<string, MissionHandler>();
        
        public event Action<MissionHandler> OnMissionStarted; 
        public event Action<MissionHandler> OnMissionStatusChanged;
        public event Action<MissionHandler> OnMissionCompleted;
        public event Action<MissionHandler> OnMissionRemoved;
        
        public bool StartMission(Mission mission)
        {
            if (mission == null || missionHandlers.ContainsKey(mission.Id)) return false;
            
            var missionHandler = new MissionHandler(mission); // todo：可对象池优化
            missionHandlers.Add(missionHandler.Id, missionHandler);
            OnMissionStarted?.Invoke(missionHandler);
            return true;
        }
        
        public void SendMessage(object message)
        {
            if (missionHandlers.Count == 0) return;

            var handlerToRemove = new Queue<MissionHandler>();
            foreach (var missionHandler in missionHandlers.Values)
            {
                // 任务没有完成
                if (!missionHandler.SendMessage(message, out bool hasStatusChanged))
                {
                    if (hasStatusChanged) OnMissionStatusChanged?.Invoke(missionHandler);
                    continue;
                }
                
                OnMissionStatusChanged?.Invoke(missionHandler);
                // 应用奖励，从任务字典中移出
                missionHandler.ApplyRewards();
                handlerToRemove.Enqueue(missionHandler);
            }
            
            while (handlerToRemove.Count > 0)
            {
                var handler = handlerToRemove.Dequeue();
                missionHandlers.Remove(handler.Id);
                OnMissionCompleted?.Invoke(handler);
            }
        }
        
        public bool RemoveMission(string missionId)
        {
            if (!missionHandlers.Remove(missionId, out var mission)) return false;
            OnMissionRemoved?.Invoke(mission);
            return true;
        }
    }
}