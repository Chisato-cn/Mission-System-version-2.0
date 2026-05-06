using System;
using UnityEngine;
using Action = Tomoe.MissionSystem.Runtime.Action;

namespace Tomoe.MissionSystem.Example
{
    [Serializable]
    public class TriggerSpawn : Action
    {
        public override string Content => "Spawn Trigger in Somewhere";

        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale;
        [SerializeField] private GameObject triggerPrefab;
        [SerializeField] private string messageGuid;
        
        public override void Execute()
        {
            var instance = GameObject.Instantiate(triggerPrefab, position, Quaternion.Euler(rotation));
            instance.transform.localScale = scale;
            var component = instance.GetComponent<IAction>();
            Debug.Log(component);
            component.Init(messageGuid);
        }
    }
}