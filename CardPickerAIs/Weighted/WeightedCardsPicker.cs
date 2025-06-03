using RoundsWithBots.CardPickerAIs.Weighted.WeightedCardProcessesor;
using RoundsWithBots.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs.Weighted {
    public class WeightedCardsPicker : ICardPickerAI {
        public List<IWeightedCardProcessor> CardProcessors { get; set; }


        public WeightedCardsPicker(List<IWeightedCardProcessor> cardProcessors) {
            CardProcessors = cardProcessors;
        }
        public WeightedCardsPicker() : this(GetDefaultWeightedCardProcessors()) { }

        public List<CardInfo> PickCard(List<CardInfo> cards) {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == CardChoice.instance.pickrID);
            Dictionary<CardInfo, float> cardWeights = new Dictionary<CardInfo, float>();

            foreach(var card in cards) {
                float weight = 1f;
                foreach(var processor in CardProcessors) {
                    weight *= processor.GetWeight(card, player);
                    LoggerUtils.Log($"Card '{card.cardName}' processed by '{processor.GetType().Name}' with weight: {weight}");
                }
                cardWeights[card] = weight;
                LoggerUtils.Log($"Card '{card.cardName}' has weight: {weight}");
            }

            List<CardInfo> sortedCards = new List<CardInfo>(cardWeights.Keys);
            sortedCards.Sort((a, b) => cardWeights[b].CompareTo(cardWeights[a]));

            if(sortedCards.Count > 0) {
                return new List<CardInfo> { sortedCards[0] };
            } else {
                return new List<CardInfo>();
            }
        }

        /// <summary>
        /// Patch this to add your own weighted card processors.
        /// </summary>
        public static List<IWeightedCardProcessor> GetDefaultWeightedCardProcessors() {
            var weightedCardProcessors = new List<IWeightedCardProcessor> {
                    new RarityWeightedCardProcessor(0.25f),
                    new StatsWeightedCardProcessor(1.25f),
                    new ThemedWeightedCardProcessor(1.5f, 0.5f),
                    new CurseWeightedCardProcessor(0.01f)
                };
            if(RoundsWithBots.Plugins.Any(plugin => plugin.Info.Metadata.GUID == "root.classes.manager.reborn")) {
                weightedCardProcessors.Add(new WeighteClassesCardProcessor(2f));
            }

            return weightedCardProcessors;
        }
    }
}
