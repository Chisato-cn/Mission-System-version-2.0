using System;
using Tomoe.MissionSystem.Runtime;
using UnityEngine;

namespace Tomoe.MissionSystem.Example
{
    /// <summary>
    /// todo：实际项目中应该在enemy模块定义
    /// </summary>
    public enum EnemyType
    {
        Goblin,
        Giant,
    }

    public class EnemyKilledMessageArg
    {
        public readonly string EnemyGuid;
        public readonly EnemyType EnemyType;
        public readonly int EnemiesKilled;

        public EnemyKilledMessageArg(string enemyGuid, EnemyType enemyType, int enemiesKilled)
        {
            EnemyGuid = enemyGuid;
            EnemyType = enemyType;
            EnemiesKilled = enemiesKilled;
        }
    }
    
    [Serializable]
    public class EnemyKilledRequirement : MissionRequirement
    {
        [SerializeField] private string enemyGuid;          // 一个类型中，不同静态配置的enemy
        [SerializeField] private EnemyType enemyType;       // 不同类型
        [SerializeField] private int enemiesKilled = 1;
        
        protected override Type handlerType => typeof(EnemyKilledRequirementHandler);
        public string EnemyGuid => enemyGuid;
        public EnemyType EnemyType => enemyType;
        public int EnemiesKilled => enemiesKilled;

        public override bool CheckMessage(object message)
        {
            if (message is not GameMessage gameMessage) return false;
            return gameMessage.Type == GameMessage.GameMessageType.EnemyKilled;
        }
    }

    public class EnemyKilledRequirementHandler : MissionRequirementHandler
    {
        private EnemyKilledRequirement enemyKilledRequirement;
        private int enemiesKilled;
        
        public EnemyKilledRequirementHandler(MissionRequirement requirement) : base(requirement)
        {
            enemyKilledRequirement = (EnemyKilledRequirement)requirement;
            enemiesKilled = 0;
        }

        protected override bool UseMessage(object message)
        {
            if (message is not GameMessage gameMessage || gameMessage.HasUsed) return false;

            EnemyKilledMessageArg args = gameMessage.Args as EnemyKilledMessageArg;
            if (args == null || 
                args.EnemyType != enemyKilledRequirement.EnemyType ||
                (string.IsNullOrEmpty(args.EnemyGuid) && args.EnemyGuid != enemyKilledRequirement.EnemyGuid))
                return false;
            
            // gameMessage.Use();       一般不需要
            enemiesKilled += args.EnemiesKilled == 0 ? 1 : args.EnemiesKilled;      // 如果参数的数量为0，默认+1
            return enemiesKilled >= enemyKilledRequirement.EnemiesKilled;
        }
    }
}