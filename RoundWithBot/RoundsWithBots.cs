using BepInEx;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using RoundsWithBots.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnboundLib.GameModes;
using UnboundLib.Utils;

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
        private List<int> BotPlayer = new List<int>();



        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start() {
            Instance = this;

            ConfigHandler.RegesterMenu(ModName, Config);

            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Remote"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Teleport"));
            CardExclusiveUtils.ExcludeCardsFromBots(CardManager.GetCardInfoWithName("Shield Charge"));
            
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, (_) => BotPicks());
            GameModeManager.AddHook(GameModeHooks.HookGameStart, (_) => RegisterBots());
        }

        IEnumerator RegisterBots() {
            BotPlayer.Clear();
            for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                Player player = PlayerManager.instance.players[i];
                if(player.GetComponent<PlayerAPI>().enabled) {
                    BotPlayer.Add(player.playerID);
                    player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
                }
            }
            BotAIManager.instance.SetBotsId();

            yield break;
        }
        IEnumerator BotPicks() {
            StartCoroutine(BotAIManager.instance.AiPickCard());

            yield break;
        }
    }
}