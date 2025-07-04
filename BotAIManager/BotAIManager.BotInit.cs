﻿using ModdingUtils.GameModes;
using RoundsWithBots.CardPickerAIs.Weighted;
using RoundsWithBots.CardPickerAIs.Weighted.WeightedCardProcessesor;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils;
using RWF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            LoggerUtils.Log("Getting bots player.");

            PickerAIs.Clear();
            List<int> botsIds = PlayerManager.instance.players
                     .Where(player => player.data.GetAdditionalData().IsBot)
                     .Select(player => player.playerID)
                     .ToList();

            PickerAIs = botsIds.ToDictionary(id => id, id => new CardPickerAI());
            if(PickerAIs.Count > 1) {
                // So we can do the "old vs new" card picking AI to see which one is better.
                PickerAIs[0].cardPickerAI = new WeightedCardsPicker();
            }

            botsIds.ForEach(id => LoggerUtils.Log($"Bot '{id}' has been added to the list of bots id."));
            LoggerUtils.Log("Successfully get list of bots player.");
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
                RoundEndHandler.instance.OnGameOverChoose("REMATCH");
            }
        }
    }
}

