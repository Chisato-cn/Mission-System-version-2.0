using System;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public class Connection
    {
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();
        [SerializeField, HideInInspector] private string inputMCNode;
        [SerializeField, HideInInspector] private string outputMCNode;
        
        [SerializeField] private bool isValid;
        [SerializeField] private bool isParallelConnection;
        
        [SerializeField] private bool hasCondition;
        [SerializeReference] private Condition[] conditions;
        
        public bool IsParallelConnection => isParallelConnection;
        
        public string Guid => guid;
        
        public bool IsAvailable
        {
            get
            {
                if (isValid) return false;
                if (!hasCondition || conditions == null) return false;
                
                return conditions.All(c => c.IsConditionMet);
            }
        }

        public string InputMCNode
        {
            get => inputMCNode;
            set => inputMCNode = value;
        }

        public string OutputMCNode
        {
            get => outputMCNode;
            set => outputMCNode = value;
        }
    }
}