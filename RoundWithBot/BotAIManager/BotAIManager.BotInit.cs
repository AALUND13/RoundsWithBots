using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using Photon.Realtime;
﻿using ModdingUtils.GameModes;
using RoundsWithBots.CardPickerAIs;
using RoundsWithBots.Utils.CardPickerAIs;
using System.Collections;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace RoundsWithBots {
    public partial class BotAIManager : MonoBehaviour, IPlayerPickStartHookHandler, IGameStartHookHandler {
        public static BotAIManager Instance;

        public ICardPickerAI defaultCardPickerAI = new RarestCardPicker();
        public Dictionary<int, ICardPickerAI> BotPickerAIs = new Dictionary<int, ICardPickerAI>();

        void Start() {
            InterfaceGameModeHooksManager.instance.RegisterHooks(this);

            DontDestroyOnLoad(this);
            Instance = this;
        }

        public void SetBotsId() {
            LoggingUtils.Log("Getting bots player.");

            BotPickerAIs.Clear();
            List<int> botsIds = PlayerManager.instance.players
                     .Where(player => player.data.GetAdditionalData().IsBot)
                     .Select(player => player.playerID)
                     .ToList();

            BotPickerAIs = botsIds.ToDictionary(id => id, id => defaultCardPickerAI);

            botsIds.ForEach(id => LoggingUtils.Log($"Bot '{id}' has been added to the list of bots id."));
            LoggingUtils.Log("Successfully get list of bots player.");
        }
        public void OnPlayerPickStart() {
            StartCoroutine(Instance.AiPickCard());
        }

        public void OnGameStart() {
            Instance.SetBotsId();
            foreach(var bot in BotPickerAIs) {
                Player player = PlayerManager.instance.players.First(p => p.playerID == bot.Key);
                player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
            }
        }
    }
}

