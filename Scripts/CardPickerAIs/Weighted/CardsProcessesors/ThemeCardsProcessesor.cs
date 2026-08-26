using System.Collections.Generic;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs.Weighted.CardsProcessesors {
    public class ThemeCardsProcessesor : WeightedCardsProcessor {
        public ThemeCardsProcessesor(float multiplier) : base(multiplier) { }

        public override WeightedCard[] GetCardsWeight(CardInfo[] cards, Player player) {
            Dictionary<CardThemeColor.CardThemeColorType, int> playerThemesAmounts = player.data.currentCards
                .GroupBy(c => c.colorTheme)
                .ToDictionary(g => g.Key, g => g.Count());

            int highestThemeAmount = playerThemesAmounts.Values.Any()
                ? playerThemesAmounts.Values.Max()
                : 0;

            if (highestThemeAmount == 0)
                return cards.Select(c => new WeightedCard(0, c)).ToArray();

            Dictionary<CardThemeColor.CardThemeColorType, float> themeWeights = playerThemesAmounts
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (float)kvp.Value / highestThemeAmount
                );

            return cards.Select(card => {
                return new WeightedCard(
                    themeWeights.TryGetValue(card.colorTheme, out float weight)
                        ? weight
                        : -1f,
                    card
                );
            }).ToArray();
        }
    }
}
