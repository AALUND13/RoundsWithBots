using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace RoundWithBot {
    [BepInDependency("dev.rounds.unbound.core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.rounds.unbound.cards", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.rounds.unbound.gamemodes", BepInDependency.DependencyFlags.HardDependency)]
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
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start() {
            instance = this;

            ConfigHandler.RegesterMenu(ModName, Config);

            utils.BotAIManager.AddExcludeCard("Remote");
            utils.BotAIManager.AddExcludeCard("Teleport");

            Unbound.Gamemodes.GameModeManager.AddHook(Unbound.Gamemodes.GameModeHooks.HookPlayerPickStart, (_) => BotPicks());
            Unbound.Gamemodes.GameModeManager.AddHook(Unbound.Gamemodes.GameModeHooks.HookGameStart, (_) => RegisterBots());

        }

        IEnumerator RegisterBots() {
            botPlayer.Clear();
            for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                Player player = PlayerManager.instance.players[i];
                if(player.GetComponent<PlayerAPI>().enabled) {
                    botPlayer.Add(player.playerID);
                    player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
                }
            }
            utils.BotAIManager.SetBotsId();
            yield break;
        }
        IEnumerator BotPicks() {
            StartCoroutine(utils.BotAIManager.AiPickCard());

            yield break;
        }
    }
}