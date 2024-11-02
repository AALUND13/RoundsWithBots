using BepInEx;
using HarmonyLib;
using RoundsWithBots.Utils;
using System.Collections.Generic;
using UnboundLib.Utils;
using UnityEngine;

namespace RoundsWithBots {
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.teleportpatch", BepInDependency.DependencyFlags.HardDependency)]

    [BepInPlugin(ModId, ModName, Version), BepInProcess("Rounds.exe")]
    public class RoundsWithBots : BaseUnityPlugin {
        private const string ModId = "com.aalund13.rounds.roundswithbots";
        private const string ModName = "Rounds With Bots";
        public const string Version = "2.3.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";

        public static RoundsWithBots Instance { get; private set; }
        public bool IsPicking = false;
        private List<int> BotPlayer = new List<int>();



        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start() {
            Instance = this;

            ConfigHandler.RegesterMenu(ModName, Config);

            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Remote"));

            BotAIManager.Instance = new GameObject($"{ModInitials}_BotAIManager").AddComponent<BotAIManager>();
        }
    }
}