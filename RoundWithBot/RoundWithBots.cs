using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RoundWithBot {
    [BepInDependency("dev.rounds.unbound.core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.rounds.unbound.cards", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.rounds.unbound.gamemodes", BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundWithBots : BaseUnityPlugin {
        private const string ModId = "com.aalund13.rounds.round_with_bot";
        private const string ModName = "Round With Bot";
        public const string Version = "2.3.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";
        public static CardCategory NoBot;

        public static RoundWithBots instance { get; private set; }
        public bool isPicking = false;
        private List<int> botPlayer = new List<int>();

        void Awake() {
            // Use this to call any harmony patch files your mod may have
            try {
                var harmony = new Harmony(ModId);
                harmony.PatchAll();
            } catch { }
        }
        void Start() {
            instance = this;

            ConfigHandler.RegesterMenu(ModName, Config);

            RWB.RoundWithBot.AddExcludeCard("Remote");

            Unbound.Gamemodes.GameModeManager.AddHook(Unbound.Gamemodes.GameModeHooks.HookPlayerPickStart, (_) => BotPicks());
            Unbound.Gamemodes.GameModeManager.AddHook(Unbound.Gamemodes.GameModeHooks.HookGameStart, (_) => RegesterBots());

        }

        IEnumerator RegesterBots() {
            botPlayer.Clear();
            for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                Player player = PlayerManager.instance.players[i];
                if(player.GetComponent<PlayerAPI>().enabled) {
                    botPlayer.Add(player.playerID);
                    player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
                }
            }
            RWB.RoundWithBot.SetBotsId();
            yield break;
        }
        IEnumerator BotPicks() {
            StartCoroutine(RWB.RoundWithBot.AiPickCard());

            yield break;
        }
    }
}