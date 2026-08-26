using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs.Weighted.CardsProcessesors {
    public class CurseWeightedCardProcessor : WeightedCardsProcessor {
        public CurseWeightedCardProcessor(float multiplier): base(multiplier) { }

        private static readonly CardCategory curseCategory = CustomCardCategories.instance.CardCategory("Curse");

        public override WeightedCard[] GetCardsWeight(CardInfo[] cards, Player player) {
            return cards
                .Select(c => c.categories.Contains(curseCategory) ? new WeightedCard(-1, c) : new WeightedCard(0, c))
                .ToArray();
        }
    }
}
