using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;

namespace RoundsWithBots.Utils {
    public class CardExclusiveUtils {
        public static CardCategory NotBotCategory = CustomCardCategories.instance.CardCategory("NotForBots");

        public static void ExcludeCardsFromBots(CardInfo card) {
            card.blacklistedCategories = card.blacklistedCategories.AddToArray(NotBotCategory);
        }
    }
}

