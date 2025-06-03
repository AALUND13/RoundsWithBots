using ClassesManagerReborn;

namespace RoundsWithBots.CardPickerAIs.Weighted.WeightedCardProcessesor {
    public class WeighteClassesCardProcessor : IWeightedCardProcessor {
        private readonly float multiplier;
        public WeighteClassesCardProcessor(float multiplier = 1f) {
            this.multiplier = multiplier;
        }

        public float GetWeight(CardInfo card, Player player) {
            ClassObject classObject = ClassesRegistry.Get(card);
            if(classObject == null) return 1f; // If the class is not found, return a default weight of 1

            switch(classObject.type) {
                case CardType.Entry:
                    return 1.5f * multiplier;
                case CardType.SubClass:
                    return 1.5f * multiplier;
                case CardType.Card:
                    return 2f * multiplier;
                default:
                    return 1f; // Default weight for other types
            }
        }
    }
}
