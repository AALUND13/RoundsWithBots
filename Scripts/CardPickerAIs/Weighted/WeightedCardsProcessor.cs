using System;

namespace RoundsWithBots.CardPickerAIs.Weighted {
    public abstract class WeightedCardsProcessor {
        public class WeightedCard {
            public float Weight;
            public CardInfo Card;

            public WeightedCard(float weight, CardInfo cardInfo) {
                Weight = weight;
                Card = cardInfo;
            }
        }

        private float multiplier;

        public WeightedCardsProcessor(float multiplier = 1) {
            this.multiplier = multiplier;
        }

        internal WeightedCard[] InternalGetCardsWeight(CardInfo[] cards, Player player) {
            WeightedCard[] result = GetCardsWeight(cards, player);

            if (result == null || result.Length != cards.Length) {
                throw new InvalidOperationException(
                    "GetCardsWeight must return exactly one WeightedCard for each input CardInfo."
                );
            }

            for (int i = 0; i < cards.Length; i++) {
                if (result[i].Card != cards[i]) {
                    throw new InvalidOperationException(
                        "GetCardsWeight returned a WeightedCard for a different card."
                    );
                }
            }

            foreach (var card in result) {
                card.Weight *= multiplier;
            }

            return result;
        }

        public abstract WeightedCard[] GetCardsWeight(CardInfo[] cards, Player player);
    }
}
