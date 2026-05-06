namespace Tomoe.MissionSystem.Example
{
    public static class GameAPI
    {
        public static Data data;

        static GameAPI()
        {
            data = new("corn");
        }
        
        public static Data Get(string targetGuid)
        {
            return data;
        }
    }

    public class Data
    {
        public readonly string TargetGuid;
        public int Amount;

        public Data(string targetGuid, int amount = 0)
        {
            TargetGuid = targetGuid;
            Amount = amount;
        }
    }
}