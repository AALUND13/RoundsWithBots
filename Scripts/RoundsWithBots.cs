using BepInEx;
using BepInEx.Logging;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using RoundsWithBots.Extensions;
using RoundsWithBots.Handlers;
using RoundsWithBots.Menu;
using RoundsWithBots.Pathfinding.Scanning;
using RoundsWithBots.Utils;
using RoundsWithBots.Utils.Debug;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnboundLib.Utils;
using UnityEngine;

namespace RoundsWithBots {
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]

    [BepInDependency("root.classes.manager.reborn", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInProcess("Rounds.exe")]
    [BepInPlugin(ModId, ModName, Version)]
    public class RoundsWithBots : BaseUnityPlugin {
        private const string ModId = "com.aalund13.rounds.roundswithbots";
        private const string ModName = "Rounds With Bots";
        public const string Version = "4.0.1"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";

        internal static List<BaseUnityPlugin> Plugins { get; private set; }
        internal static RoundsWithBots Instance { get; private set; }
        internal static ManualLogSource ModLogger { get; private set; }

        public AssetBundle Assets;

        void Awake() {
            Instance = this;
            ModLogger = Logger;

            Assets = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("rwb_assets", typeof(RoundsWithBots).Assembly);
            gameObject.AddComponent<DebugVisualizer>();

            RWBMenu.BindConfigs(ModName, Config);

            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }

        void Start() {
            Plugins = (List<BaseUnityPlugin>)typeof(BepInEx.Bootstrap.Chainloader).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            RWBMenu.RegisterMenu(ModName);

            ModdingUtils.Utils.Cards.instance.AddCardValidationFunction((player, card) => {
                if (player.data.GetAdditionalData().IsBot && card.blacklistedCategories.Contains(CustomCardCategories.instance.CardCategory("NotForBots"))) 
                    return false;

                return true;
            });

            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Remote"));

            gameObject.AddComponent<BotsManager>();
            gameObject.AddComponent<BotCardPickerHandler>();
            gameObject.AddComponent<BotStalemateHandler>();
            gameObject.AddComponent<MapScanner>();
        }
    }
}