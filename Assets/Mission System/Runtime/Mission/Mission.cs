using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public enum MissionRequirementMode
    {
        All,        // 全部完成
        Any,        // 任一完成
        Custom,     // 自定义完成
        Sequence,   // 顺序完成
    }

    [Serializable]
    public class Mission
    {
        [SerializeField] private string missionId;
        [SerializeField] private string missionName;
        [SerializeField] private string missionDescription;
        [SerializeField] private MissionRequirementMode missionRequirementMode;
        [SerializeField] private int customRequirementCompleteCount;
        [SerializeReference] private MissionRequirement[] missionRequirements;
        [SerializeReference] private MissionReward[] missionRewards;
        
        public string Id => missionId;
        public string Name => missionName;
        public string Description => missionDescription;
        public bool IsSingleRequirement => missionRequirements.Length == 1;
        public MissionRequirementMode RequirementMode => missionRequirementMode;
        public int CustomRequirementCompleteCount => customRequirementCompleteCount;
        public IReadOnlyList<MissionRequirement> Requirements => missionRequirements;
        public IReadOnlyList<MissionReward> Rewards => missionRewards;

        public Mission(string missionId) => this.missionId = missionId;
        
        public void ApplyRewards()
        {
            if (missionRewards is null || missionRewards.Length == 0) return;
            foreach (var reward in missionRewards)
            {
                reward.ApplyReward();
            }
        }
    }
}