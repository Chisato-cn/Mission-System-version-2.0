namespace Tomoe.MissionSystem.Example
{
    public class GameMessage
    {
        public enum GameMessageType
        {
            DialogueCompleted,
            EnemyKilled,
            ItemCollected,
            SubMissionChainCompleted,
            StringArgsCompare,
        }
        public GameMessageType Type { get; private set; }
        public object Args { get; private set; }
        public bool HasUsed { get; private set; }
        
        public void Init(GameMessageType type, object args = null)
        {
            Type = type;
            Args = args;
            HasUsed = false;
        }
        
        public void Use() => HasUsed = true;
    }
}