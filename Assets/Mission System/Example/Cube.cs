using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    public class Cube : MonoBehaviour, IAction
    {
        private string message;
        
        private void OnTriggerEnter(Collider other)
        {
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.StringArgsCompare, message);
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log("Trigger");
        }

        public void Init(string message)
        {
            this.message = message;
            Debug.Log(message);
        }
    }

    public interface IAction
    {
        void Init(string message);
    }
}