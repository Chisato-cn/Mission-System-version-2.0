using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public class Connection
    {
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();
        [SerializeReference, HideInInspector] private MCNode inputMCNode;
        [SerializeReference, HideInInspector] private MCNode outputMCNode;
        
        [SerializeField] private bool isValid;
        [SerializeField] private bool isParallelConnection;
        
        [SerializeField] private bool hasCondition;
        [SerializeReference] private Condition condition;
        
        public bool IsParallelConnection => isParallelConnection;
        
        public string Guid => guid;
        
        public bool IsAvailable
        {
            get
            {
                if (isValid) return false;
                if (!hasCondition || condition == null) return false;
                return condition.IsConditionMet;
            }
        }

        public MCNode InputMCNode
        {
            get => inputMCNode;
            set => inputMCNode = value;
        }

        public MCNode OutputMCNode
        {
            get => outputMCNode;
            set => outputMCNode = value;
        }
    }
}