using BepInEx;
using HarmonyLib;
using Photon.Pun;
using RoundsWithBots.Menu;
using RoundsWithBots.Utils;
using UnboundLib;
using UnboundLib.Networking;
using UnboundLib.Utils;
using UnityEngine;

namespace RoundsWithBots {
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]

    [BepInPlugin(ModId, ModName, Version), BepInProcess("Rounds.exe")]
    public class RoundsWithBots : BaseUnityPlugin {
        private const string ModId = "com.aalund13.rounds.roundswithbots";
        private const string ModName = "Rounds With Bots";
        public const string Version = "2.3.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";

        public static RoundsWithBots Instance { get; private set; }
        public bool IsPicking = false;

        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start() {
            Instance = this;

            RWBMenu.RegisterMenu(ModName, Config);

            Unbound.RegisterHandshake(ModId, OnHandShakeCompleted);

            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Remote"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Teleport"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Shield Charge"));

            BotAIManager.Instance = new GameObject($"{ModInitials}_BotAIManager").AddComponent<BotAIManager>();
        }

        private void OnHandShakeCompleted() {
            if(PhotonNetwork.IsMasterClient) {
                NetworkingManager.RPC_Others(
                    GetType(), 
                    nameof(RPCA_SyncSettings), 

                    RWBMenu.StalemateTimer.Value, 
                    RWBMenu.StalemateDamageCooldown.Value, 
                    RWBMenu.StalemateDamageDuration.Value,
                    
                    RWBMenu.CycleDelay.Value,
                    RWBMenu.PreCycleDelay.Value,
                    RWBMenu.GoToCardDelay.Value,
                    RWBMenu.PickDelay.Value
                );
            }
        }

        [UnboundRPC]
        private static void RPCA_SyncSettings(
            float stalemateTimer, 
            float stalemateDamageCooldown, 
            float stalemateDamageDuration, 
            float cycleDelay, 
            float preCycleDelay, 
            float goToCardDelay, 
            float pickDelay
        ) {
            RWBMenu.StalemateTimer.Value = stalemateTimer;
            RWBMenu.StalemateDamageCooldown.Value = stalemateDamageCooldown;
            RWBMenu.StalemateDamageDuration.Value = stalemateDamageDuration;

            RWBMenu.CycleDelay.Value = cycleDelay;
            RWBMenu.PreCycleDelay.Value = preCycleDelay;
            RWBMenu.GoToCardDelay.Value = goToCardDelay;
            RWBMenu.PickDelay.Value = pickDelay;
        }
    }
}