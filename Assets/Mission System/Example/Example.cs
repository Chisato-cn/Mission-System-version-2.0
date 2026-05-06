using Tomoe.MissionSystem.Example;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class Example : MonoBehaviour
{
    public MissionChain missionChain;
    
    void Start()
    {
        MissionChainSystem.Instance.StartChain(missionChain);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            // dialouge-111
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.DialogueCompleted, "dialogue-111");
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + dialogue-111");
        }
        
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            // dialouge-222
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.DialogueCompleted, "dialogue-222");
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + dialogue-222");
        }
        
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            // dialouge-222
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.DialogueCompleted, "dialogue-159");
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + dialogue-159");
        }
        
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // enemy-111
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.EnemyKilled, new EnemyKilledMessageArg("enemy-111", EnemyType.Goblin, 2));
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + enemy-111 + 2");
        }
        
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            // aotuo
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.EnemyKilled, new EnemyKilledMessageArg("aotuo", EnemyType.Giant, 1));
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + aotuo + 1");
        }
        
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // corn
            var gameMessage = new GameMessage();
            gameMessage.Init(GameMessage.GameMessageType.ItemCollected, new ItemCollectedMessageArg("corn", 100));
            
            MissionChainSystem.Instance.MissionSystem.SendMessage(gameMessage);
            Debug.Log($"发送消息:{gameMessage.Type} + corn + 100");
        }
    }
}
