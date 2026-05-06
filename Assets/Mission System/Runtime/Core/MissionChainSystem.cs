using System.Collections.Generic;
using Tomoe.MissionSystem.Example;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

            Init();
        }

        #endregion
        
        private MissionSystem missionSystem;
        private Dictionary<string, MissionChainHandler> allChainHandlers;
        
        public MissionSystem MissionSystem => missionSystem;
        
        private void Init()
        {
            missionSystem = new MissionSystem();
            allChainHandlers = new Dictionary<string, MissionChainHandler>();
            
            missionSystem.OnMissionsRemoved += MissionsSystemOnOnMissionsRemoved;
        }

        public void StartChain(MissionChain chain)
        {
            if (chain == null || allChainHandlers.ContainsKey(chain.Guid)) return;

            var handler = new MissionChainHandler();
            handler.OnRemoveParallelMission = missionId => missionSystem.RemoveMission(missionId);
            handler.OnStartSubMissionChain = subChainId => StartChain(LoadMissionChain(subChainId));
            handler.Init(chain);
            
            handler.FlushBuffer(mission => missionSystem.StartMission(mission));
            if (!handler.IsCompleted) allChainHandlers.Add(chain.Guid, handler);
        }

        private MissionChain LoadMissionChain(string chainId)
        {
            return Addressables.LoadAssetAsync<MissionChain>(chainId).WaitForCompletion();
        }
        
        private void MissionsSystemOnOnMissionsRemoved(List<MissionHandler> missionHandlers, bool isCompleted)
        {
            var dict = new Dictionary<string, List<MissionHandler>>();
            
            foreach (var handler in missionHandlers)
            {
                string chainId = handler.Id.Split('|')[0];
                if (!allChainHandlers.ContainsKey(chainId)) continue;

                if (dict.TryGetValue(chainId, out var handlers))
                {
                    handlers ??= new List<MissionHandler>();
                    handlers.Add(handler);
                }
                else dict.Add(chainId, new List<MissionHandler> { handler });
            }
            
            foreach (var pair in dict)
            {
                var chainHandler = allChainHandlers[pair.Key];
                chainHandler.OnMissionsRemoved(pair.Value, isCompleted);
                chainHandler.FlushBuffer(mission => missionSystem.StartMission(mission));

                if (chainHandler.IsCompleted)
                {
                    allChainHandlers.Remove(pair.Key);

                    var message = new GameMessage();
                    message.Init(GameMessage.GameMessageType.SubMissionChainCompleted, pair.Key);
                    missionSystem.SendMessage(message);
                }
            }
        }
    }
}