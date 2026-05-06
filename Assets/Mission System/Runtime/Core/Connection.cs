using System;
using System.Linq;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public class Connection
    {
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();
        [SerializeField] private string inputMCNode;
        [SerializeField] private string outputMCNode;
        
        [SerializeField] private bool isNotValid;
        [SerializeField] private bool isParallelConnection;
        
        [SerializeField] private bool hasCondition;
#if UNITY_EDITOR
        [CustomPropertyDrawerType("Tomoe.MissionSystem.Editor.SerializedReferenceListPropertyDrawer")]
#endif
        [SerializeReference, HideInInspector] private Condition[] conditions;
        
        public bool IsParallelConnection => isParallelConnection;
        
        public string Guid => guid;
        
        public bool IsAvailable
        {
            get
            {
                if (isNotValid) return false;
                if (!hasCondition || conditions == null) return true;
                
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