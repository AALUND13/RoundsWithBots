using RarityLib.Utils;
using System;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs.Weighted.CardsProcessesors {
    public class RarityCardsProcessesor : WeightedCardsProcessor {
        public RarityCardsProcessesor(float multiplier) : base(multiplier) { }

        public override WeightedCard[] GetCardsWeight(CardInfo[] cards, Player player) {
            var raw = cards
                .Select(c => 1f / Math.Max(
                    1e-6f,
                    RarityUtils.GetRarityData(c.rarity).calculatedRarity
                ))
                .ToList();

            float max = raw.Max();
            return cards
                .Select((card, i) => new WeightedCard(raw[i] / max, card))
                .ToArray();
        }
    }
}
