using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    public class MissionChainSystem : MonoBehaviour
    {
        #region Singleton

        public static MissionChainSystem Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion
        
        
    }
}