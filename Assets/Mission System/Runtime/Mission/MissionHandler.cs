using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public class MissionHandler
    {
        private Mission mission;
        private List<MissionRequirementHandler> requirementHandlers;
        
        public string Id => mission.Id;

        public void Init(Mission mission)
        {
            this.mission = mission;
            requirementHandlers = mission.Requirements.Select(requirement => requirement.CreateHandler()).ToList();
        }

        public bool SendMessage(object message, out bool hasStatusChanged)
        {
            if (mission.IsSingleRequirement) return requirementHandlers[0].SendMessage(message, out hasStatusChanged);
            
            // 多个任务需求
            switch (mission.RequirementMode)
            {
                case MissionRequirementMode.Sequence:
                    return SendMessageBasedOnSequenceRequirementMode(message, out hasStatusChanged);
                case MissionRequirementMode.Any:
                    return SendMessageBasedOnAnyRequirementMode(message, out hasStatusChanged);
                case MissionRequirementMode.Custom:
                    return SendMessageBasedOnCustomRequirementMode(message, out hasStatusChanged);
                case MissionRequirementMode.All:
                    return SendMessageBasedOnAllRequirementMode(message, out hasStatusChanged);
            }
            
            Debug.LogError("未知需求类型，或存在逻辑未更新！");
            hasStatusChanged = false;
            return false;
        }
        
        private bool SendMessageBasedOnSequenceRequirementMode(object message, out bool hasStatusChanged)
        {
            hasStatusChanged = false;
            
            while (requirementHandlers.Count > 0)
            {
                var handler = requirementHandlers[0];
                
                if (!handler.SendMessage(message, out bool handlerHasStatusChanged))
                {
                    hasStatusChanged |= handlerHasStatusChanged;
                    Debug.Log(handler.GetType() + "未完成需求");
                    return false;  // 当前处理器未完成，保持它在队首
                }
        
                Debug.Log(handler.GetType() + "完成这个需求,推进");
                hasStatusChanged = true;
                requirementHandlers.RemoveAt(0);  // 当前处理器完成，出队
            }
            
            return true;
        }
        
        private bool SendMessageBasedOnAnyRequirementMode(object message, out bool hasStatusChanged)
        {
            hasStatusChanged = false;

            foreach (var requirementHandler in requirementHandlers)
            {
                // 任务需求未完成，但需求状态发生改变
                if (!requirementHandler.SendMessage(message, out bool subHasStatusChanged))
                {
                    // 或运算，只要有一个为true，那就是true
                    hasStatusChanged |= subHasStatusChanged;
                    continue;
                }
                
                // 需求状态发生改变，同时任务需求完成
                hasStatusChanged = true;
                return true;
            }
            
            return false;
        }

        private bool SendMessageBasedOnCustomRequirementMode(object message, out bool hasStatusChanged)
        {
            SendMessageToMultiRequirement(message, out hasStatusChanged);
            return requirementHandlers.Count == (mission.Requirements.Count - mission.CustomRequirementCompleteCount);
        }

        private bool SendMessageBasedOnAllRequirementMode(object message, out bool hasStatusChanged)
        {
            SendMessageToMultiRequirement(message, out hasStatusChanged);
            return requirementHandlers.Count == 0;
        }

        private void SendMessageToMultiRequirement(object message, out bool hasStatusChanged)
        {
            hasStatusChanged = false;
            Queue<MissionRequirementHandler> queueToRemove = new Queue<MissionRequirementHandler>();
            
            foreach (var requirementHandler in requirementHandlers)
            {
                // 任务需求未完成，但需求状态发生改变
                if (!requirementHandler.SendMessage(message, out bool subHasStatusChanged))
                {
                    // 或运算，只要有一个为true，那就是true
                    hasStatusChanged |= subHasStatusChanged;
                    continue;
                }
                
                // 需求状态发生改变，同时任务需求完成
                hasStatusChanged = true;
                queueToRemove.Enqueue(requirementHandler);
            }
            
            while (queueToRemove.Count > 0)
            {
                var handle = queueToRemove.Dequeue();
                requirementHandlers.Remove(handle);
            }
        }
        
        public void ApplyRewards() => mission.ApplyRewards();
    }
}