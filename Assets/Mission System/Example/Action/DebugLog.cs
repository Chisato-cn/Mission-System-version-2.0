using System;
using UnityEngine;
using Action = Tomoe.MissionSystem.Runtime.Action;

namespace Tomoe.MissionSystem.Example
{
    [Serializable]
    public class DebugLog : Action
    {
        public override string Content => "Log a message to the console based on logType.";
        
        [SerializeField] private string message;
        [SerializeField] private LogType logType = LogType.Log;
        
        public override void Execute()
        {
            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new System.Exception(message));
                    break;
            }
        }
    }
}