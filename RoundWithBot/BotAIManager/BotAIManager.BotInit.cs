using ModdingUtils.GameModes;
using RoundsWithBots.CardPickerAIs;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RoundsWithBots {
    public partial class BotAIManager : MonoBehaviour, IPlayerPickStartHookHandler, IGameStartHookHandler, IPointStartHookHandler {
        public static BotAIManager Instance;

        public ICardPickerAI defaultCardPickerAI = new RarestCardPicker();
        public Dictionary<int, ICardPickerAI> BotPickerAIs = new Dictionary<int, ICardPickerAI>();

        private Coroutine stalemateHandlerCoroutine;

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

        public void OnPointStart() {
            if(stalemateHandlerCoroutine != null) {
                StopCoroutine(stalemateHandlerCoroutine);
            }

            stalemateHandlerCoroutine = StartCoroutine(StalemateHandler.HandleStalemate());
        }
    }
}

