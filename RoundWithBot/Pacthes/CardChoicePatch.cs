using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoundWithBot.Patches.RWF {
    public static class SpawnedCardsHolder {
        public static List<CardInfo> spawnedCards = new List<CardInfo>();
    }

    [HarmonyPatch(typeof(CardChoice))]
    public static class CardChoicePatch {
        private static List<CardInfo> GetSpawnabeCards() {
            List<CardInfo> enableCards = CardChoice.instance.cards.ToList();
            List<CardInfo> spawnabeCards = enableCards.Except(SpawnedCardsHolder.spawnedCards).Except(utils.BotAIManager.excludeCards).ToList();
            return spawnabeCards;
        }


        [HarmonyPatch("SpawnUniqueCard")]
        [HarmonyPostfix]
        private static void AddSpawnedCard(GameObject __result) {
            SpawnedCardsHolder.spawnedCards.Add(__result.GetComponent<CardInfo>().sourceCard);
        }

        [HarmonyPatch("IDoEndPick")]
        [HarmonyPrefix]
        private static void ClearSpawnedCards() {
            SpawnedCardsHolder.spawnedCards.Clear();
        }

        [HarmonyPatch("GetRandomCard")]
        [HarmonyPostfix]
        private static void IgnoreExcludedCards(ref GameObject __result) {
            if(GetSpawnabeCards().Count != 0 && utils.BotAIManager.IsAExcludeCard(__result.GetComponent<CardInfo>())) {
                GameObject card = (GameObject)AccessTools.Method(typeof(CardChoice), "GetRandomCard").Invoke(CardChoice.instance, null);
                __result = card;
            }
        }
    }
}

