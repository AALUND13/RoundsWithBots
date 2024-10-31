using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using ModdingUtils.Utils;
using System;
using System.Linq;

namespace RoundsWithBots.Pacthes.ModdingUtils {
    [HarmonyPatch(typeof(Cards))]
    internal class PlayerIsAllowedCard {
        [HarmonyPatch("PlayerIsAllowedCard")]
        [HarmonyPostfix]
        public static void Postfix(Player player, CardInfo card, ref bool __result, Cards __instance) {
            if(player.GetComponent<PlayerAPI>().enabled && card.blacklistedCategories.Contains(CustomCardCategories.instance.CardCategory("NotForBots"))) __result = false;
        }
    }
}
