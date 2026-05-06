using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public class MissionSystem
    {
        private Dictionary<string, MissionHandler> missionHandlers = new Dictionary<string, MissionHandler>();
        
        public event Action<MissionHandler> OnMissionStarted; 
        public event Action<MissionHandler, bool> OnMissionStatusChanged;
        public event Action<List<MissionHandler>, bool> OnMissionsRemoved;
        
        public bool StartMission(Mission mission)
        {
            if (mission is null || missionHandlers.ContainsKey(mission.Id)) return false;
            
            var handler = new MissionHandler();
            handler.Init(mission);
            missionHandlers.Add(mission.Id, handler);
            
            OnMissionStarted?.Invoke(handler);
            Debug.Log(mission.Name);
            return true;
        }

        public bool RemoveMission(string missionId)
        {
            if (!missionHandlers.Remove(missionId, out var mission)) return false;
            OnMissionsRemoved?.Invoke(new List<MissionHandler>(){ mission }, false);
            return true;
        }
        
        public void SendMessage(object message)
        {
            if (missionHandlers.Count == 0) return;

            Queue<MissionHandler> messageQueue = new Queue<MissionHandler>();
            foreach (var handler in missionHandlers.Values)
            {
                // false代表,任务没完成, hasStatusChanged代表任务需求的状态发生了变化（有可能是进入下一个需求，也有可能是需求的进度推进）
                if (!handler.SendMessage(message, out bool hasStatusChanged))
                {
                    if (hasStatusChanged) OnMissionStatusChanged?.Invoke(handler, false);
                    continue;
                }
                
                // true代表，当前任务完成
                OnMissionStatusChanged?.Invoke(handler, true);
                handler.ApplyRewards();
                messageQueue.Enqueue(handler);
            }
            
            var hashSet = messageQueue.ToList();
            while (messageQueue.Count > 0)
            {
                var handler = messageQueue.Dequeue();
                missionHandlers.Remove(handler.Id);
            }
            if (hashSet.Count > 0) OnMissionsRemoved?.Invoke(hashSet, true);
        }
    }
}