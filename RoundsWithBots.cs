using BepInEx;
using BepInEx.Logging;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using Photon.Pun;
using RoundsWithBots.Menu;
using RoundsWithBots.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public const string Version = "3.2.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";

        internal static List<BaseUnityPlugin> Plugins { get; private set; }
        internal static RoundsWithBots Instance { get; private set; }
        internal static ManualLogSource ModLogger { get; private set; }

        public bool IsPicking = false;

        public AssetBundle Assets;

        void Awake() {
            Instance = this;
            ModLogger = Logger;

            Assets = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("rwb_assets", typeof(RoundsWithBots).Assembly);

            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
        void Start() {
            Plugins = (List<BaseUnityPlugin>)typeof(BepInEx.Bootstrap.Chainloader).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            RWBMenu.RegisterMenu(ModName, Config);

            Unbound.RegisterHandshake(ModId, OnHandShakeCompleted);

            ModdingUtils.Utils.Cards.instance.AddCardValidationFunction((player, card) => {
                if(player.GetComponent<PlayerAPI>().enabled
                   && card.blacklistedCategories.Contains(CustomCardCategories.instance.CardCategory("NotForBots"))
                ) return false;

                return true;
            });

            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Remote"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Teleport"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Shield Charge"));

            BotAIManager.Instance = new GameObject($"{ModInitials}_BotAIManager").AddComponent<BotAIManager>();
            DontDestroyOnLoad(BotAIManager.Instance.gameObject);
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