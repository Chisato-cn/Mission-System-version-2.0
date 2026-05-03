using System;
using System.Collections.Generic;
using Tomoe.MissionSystem.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Tomoe.MissionSystem.Editor
{
    [MCNodeView(NodeType.Mission)]
    public class MCMissionNodeView : MissionChainNode
    {
        private Mission mission;

        private MissionExtensionItemStyle_TitlePlusContent missionDescription;
        private MissionExtensionItemStyle_LabelPlusValue missionRequirementMode;
        private MissionExtensionItemStyle_LabelPlusValue missionCustomRequirement;
        private List<MissionExtensionItemStyle_IconPlusContent> missionRequirements;
        private List<MissionExtensionItemStyle_IconPlusContent> missionRewards;
        
        public MCMissionNodeView(MCNode node, Func<EdgeConnectorListener> onCreateEdgeConnector) : base(node, onCreateEdgeConnector)
        {
            CreatePort("output", Direction.Output);
            CreatePort("input", Direction.Input);
            AddToClassList("mission-node");

            mission = ((MCMissionNode)node).Mission;
            missionRequirements = new List<MissionExtensionItemStyle_IconPlusContent>();
            missionRewards = new List<MissionExtensionItemStyle_IconPlusContent>();

            missionDescription = new MissionExtensionItemStyle_TitlePlusContent("Description", mission.Description);
            extensionContainer.Add(missionDescription);
            
            missionRequirementMode = new MissionExtensionItemStyle_LabelPlusValue("Requirement Mode: ", mission.RequirementMode.ToString());
            extensionContainer.Add(missionRequirementMode);

            if (mission.RequirementMode == MissionRequirementMode.Custom)
            {
                missionCustomRequirement = new MissionExtensionItemStyle_LabelPlusValue("Custom Requirement Complete Amount: ", mission.CustomRequirementCompleteCount.ToString());
                extensionContainer.Add(missionCustomRequirement);
            }
            
            foreach (MissionRequirement requirement in mission.Requirements)
            { 
                var item = new MissionExtensionItemStyle_IconPlusContent(Resources.Load<Texture2D>("Icon/cs Script Icon"), requirement.Description);
                missionRequirements.Add(item);
                extensionContainer.Add(item);
            }

            foreach (MissionReward reward in mission.Rewards)
            {
                var item = new MissionExtensionItemStyle_IconPlusContent(Resources.Load<Texture2D>(reward.IconPath), reward.ItemName, reward.ItemAmount.ToString());
                missionRewards.Add(item);
                extensionContainer.Add(item);
            }
            
            title = $"【Mission】 {mission.Name}";
            RefreshExpandedState();
        }

        public override void UpdateView()
        {
            title = $"【Mission】 {mission.Name}";
            
            missionDescription.UpdateView("Description", mission.Description);
            missionRequirementMode.UpdateView("Requirement Mode: ", mission.RequirementMode.ToString());
            missionCustomRequirement?.UpdateView("Custom Requirement Complete Amount: ", mission.CustomRequirementCompleteCount.ToString());

            for (int i = 0; i < missionRequirements.Count; i++)
            {
                missionRequirements[i].UpdateView(Resources.Load<Texture2D>("Icon/cs Script Icon"), mission.Requirements[i].Description);
            }
            
            for (int i = 0; i < missionRewards.Count; i++)
            {
                missionRewards[i].UpdateView(Resources.Load<Texture2D>(mission.Rewards[i].IconPath), mission.Rewards[i].ItemName, mission.Rewards[i].ItemAmount.ToString());
            }
            
            RefreshExpandedState();
        }
    }
}