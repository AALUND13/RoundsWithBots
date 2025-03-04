using HarmonyLib;
using ModdingUtils.GameModes;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnboundLib.GameModes;
using UnityEngine;

namespace RoundsWithBots {
    public partial class BotAIManager : MonoBehaviour, IPlayerPickStartHookHandler, IGameStartHookHandler, IPointStartHookHandler, IRoundEndHookHandler {
        public static BotAIManager Instance;

        public Dictionary<int, CardPickerAI> PickerAIs = new Dictionary<int, CardPickerAI>();

        private Coroutine stalemateHandlerCoroutine;

        void Start() {
            InterfaceGameModeHooksManager.instance.RegisterHooks(this);

            DontDestroyOnLoad(this);
            Instance = this;
        }

        public void SetBotsId() {
            LoggingUtils.Log("Getting bots player.");

            PickerAIs.Clear();
            List<int> botsIds = PlayerManager.instance.players
                     .Where(player => player.data.GetAdditionalData().IsBot)
                     .Select(player => player.playerID)
                     .ToList();

            PickerAIs = botsIds.ToDictionary(id => id, id => new CardPickerAI());

            botsIds.ForEach(id => LoggingUtils.Log($"Bot '{id}' has been added to the list of bots id."));
            LoggingUtils.Log("Successfully get list of bots player.");
        }

        public void OnPlayerPickStart() {
            StartCoroutine(Instance.AiPickCard());
        }

        public void OnGameStart() {
            Instance.SetBotsId();
            foreach(var bot in PickerAIs) {
                Player player = PlayerManager.instance.players.First(p => p.playerID == bot.Key);
                player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
            }
        }

        public void OnPointStart() {
            if(stalemateHandlerCoroutine != null) {
                StopCoroutine(stalemateHandlerCoroutine);
            }

            stalemateHandlerCoroutine = StartCoroutine(StalemateHandler.HandleStalemate());
        }

        public void OnRoundEnd() {
            int maxRounds = (int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
            var teams = PlayerManager.instance.players.Select(p => p.teamID).Distinct();
            int? winnerTeam = teams.Select(id => (int?)id).FirstOrDefault(id => GameModeManager.CurrentHandler.GetTeamScore(id.Value).rounds >= maxRounds);
        
            bool isAllBot = PlayerManager.instance.players.All(p => p.data.GetAdditionalData().IsBot
                || ModdingUtils.AIMinion.Extensions.CharacterDataExtension.GetAdditionalData(p.data).isAIMinion);

            if(winnerTeam != null && isAllBot) {
                StartCoroutine(Rematch());
            }
        }

        public IEnumerator Rematch() {
            yield return new WaitForSeconds(1f);

            // Just to be safe, we check if the game is still playing, after the delay.
            if(GameManager.instance.isPlaying) {
                // Since the 'RoundEndHandler' is a internal class, we need to use 'AccessTools' to get the method.
                Type type = AccessTools.TypeByName("RWF.RoundEndHandler");
                object instance = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                type.GetMethod("OnGameOverChoose", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[] { "REMATCH" });
            }
        }
    }
}

