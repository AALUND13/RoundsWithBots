using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using RarityLib.Utils;
using RoundsWithBots.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoundsWithBots.CardPickerAIs.Weighted.WeightedCardProcessesor {
    public class RarityWeightedCardProcessor : IWeightedCardProcessor {
        private readonly float multiplier;

        public RarityWeightedCardProcessor(float multiplier = 0.25f) {
            this.multiplier = multiplier;
        }

        public float GetWeight(CardInfo card, Player player) {
            return 1 / RarityUtils.GetRarityData(card.rarity).calculatedRarity * multiplier;
        }
    }

    public class StatsWeightedCardProcessor : IWeightedCardProcessor {
        private readonly float multiplier;

        public StatsWeightedCardProcessor(float multiplier = 1.25f) {
            this.multiplier = multiplier;
        }

        public float GetWeight(CardInfo card, Player player) {
            float statsWeight = 1f;
            foreach(var stat in card.cardStats) {
                if(stat.positive) {
                    statsWeight *= multiplier;
                } else {
                    statsWeight *= 1 / multiplier;
                }
            }

            return statsWeight;
        }
    }

    /// <summary>
    /// This make the bot try to pick cards that match the themes of the cards it already has.
    /// </summary>
    public class ThemedWeightedCardProcessor : IWeightedCardProcessor {
        private readonly float multiplier;
        private readonly float minimumWeight;

        public ThemedWeightedCardProcessor(float multiplier = 1.5f, float minimumWeight = 0.1f) {
            this.multiplier = multiplier;
            this.minimumWeight = minimumWeight;
        }

        public float GetWeight(CardInfo card, Player player) {
            Dictionary<CardThemeColor.CardThemeColorType, int> playerThemesAmounts = player.data.currentCards
                .GroupBy(c => c.colorTheme)
                .ToDictionary(g => g.Key, g => g.Count());

            LoggerUtils.Log($"Player {player.playerID} themes: {string.Join(", ", playerThemesAmounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

            int highestThemeAmount = playerThemesAmounts.Values.Any() ? playerThemesAmounts.Values.Max() : 0;
            if(highestThemeAmount == 0) return 1; // No themes to compare against  

            LoggerUtils.Log($"Highest theme amount for player {player.playerID}: {highestThemeAmount}");

            Dictionary<CardThemeColor.CardThemeColorType, float> themeWeights = playerThemesAmounts
                .ToDictionary(kvp => kvp.Key, kvp => (float)kvp.Value / highestThemeAmount);

            LoggerUtils.Log($"Player {player.playerID} theme weights: {string.Join(", ", themeWeights.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

            if(themeWeights.TryGetValue(card.colorTheme, out float themeWeight)) {
                LoggerUtils.Log($"Card '{card.cardName}' matches theme '{card.colorTheme}' with weight: {themeWeight}");

                return Mathf.Max(minimumWeight, (themeWeight + 1) * multiplier);
            }

            return minimumWeight;
        }
    }

    public class CurseWeightedCardProcessor : IWeightedCardProcessor {
        private static readonly CardCategory curseCategory = CustomCardCategories.instance.CardCategory("Curse");
        private readonly float curseMultiplier;

        public CurseWeightedCardProcessor(float curseMultiplier = 0.01f) {
            this.curseMultiplier = curseMultiplier;
        }

        public float GetWeight(CardInfo card, Player player) {
            if(card.categories.Contains(curseCategory)) {
                return curseMultiplier;
            }
            return 1f;
        }
    }
}
