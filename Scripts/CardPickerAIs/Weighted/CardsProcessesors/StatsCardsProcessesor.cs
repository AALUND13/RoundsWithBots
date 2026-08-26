using System.Linq;
using UnityEngine;

namespace RoundsWithBots.CardPickerAIs.Weighted.CardsProcessesors {
    public class StatsCardsProcessesor : WeightedCardsProcessor {
        public StatsCardsProcessesor(float multiplier) : base(multiplier) { }

        public override WeightedCard[] GetCardsWeight(CardInfo[] cards, Player player) {
            var raw = cards
                .Select(c => c.cardStats.Sum(s => s.positive ? 1f : -1f))
                .ToList();

            float maxAbsolute = raw.Count > 0
                ? raw.Max(Mathf.Abs)
                : 0f;

            if (maxAbsolute == 0f) {
                return cards
                    .Select(card => new WeightedCard(0f, card))
                    .ToArray();
            }

            return cards
                .Select((card, i) => new WeightedCard(
                    raw[i] / maxAbsolute,
                    card
                ))
                .ToArray();
        }
    }
}
