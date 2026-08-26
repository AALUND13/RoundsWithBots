using ModdingUtils.GameModes;
using RoundsWithBots.Extensions;
using RWF;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RoundsWithBots.Handlers {
    public class BotsManager : MonoBehaviour, IGameStartHookHandler {
        public static BotsManager Instance { get; private set; }

        private Player[] botList = new Player[0];
        public IReadOnlyCollection<Player> BotList => new ReadOnlyCollection<Player>(botList);

        private void Awake() {
            if (Instance != null) {
                Destroy(Instance);
                return;
            } else {
                Instance = this;
            }

            InterfaceGameModeHooksManager.instance.RegisterHooks(this);
        }


        public void FindBots() {
            botList = PlayerManager.instance.players
                     .Where(player => player.data.GetAdditionalData().IsBot)
                     .ToArray();
        }

        public void OnGameStart() {
            Instance.FindBots();
            foreach (var bot in botList) {
                Player player = PlayerManager.instance.players.First(p => p == bot);
                player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
            }
        }
    }
}

