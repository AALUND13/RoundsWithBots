using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using Photon.Realtime;
using RoundsWithBots.CardPickerAIs;
using RoundsWithBots.Utils.CardPickerAIs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace RoundsWithBots.Utils
{
    public partial class BotAIManager : MonoBehaviour {
        private static BotAIManager _instance;
        public static BotAIManager instance {
            get {
                if(_instance == null) {
                    _instance = new GameObject("BotAIManager").AddComponent<BotAIManager>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        public ICardPickerAI defaultCardPickerAI = new RarestCardPicker();
        public Dictionary<int, ICardPickerAI> BotPickerAIs = new Dictionary<int, ICardPickerAI>();

        public void SetBotsId() {
            Logger.Log("Getting bots player.");

            BotPickerAIs.Clear();
            List<int> botsIds = PlayerManager.instance.players
                     .Where(player => player.GetComponent<PlayerAPI>().enabled)
                     .Select(player => player.playerID)
                     .ToList();

            BotPickerAIs = botsIds.ToDictionary(id => id, id => defaultCardPickerAI);

            botsIds.ForEach(id => Logger.Log($"Bot '{id}' has been added to the list of bots id."));
            Logger.Log("Successfully get list of bots player.");
        }
    }
}

