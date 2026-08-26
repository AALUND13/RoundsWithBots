using BepInEx.Logging;
using RoundsWithBots.CardPickerAIs.Weighted.CardsProcessesors;
using RoundsWithBots.Utils.Debug;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs.Weighted {
    public class WeightedCardsPicker : ICardPickerAI {
        private List<WeightedCardsProcessor> CardProcessors;

        public WeightedCardsPicker(List<WeightedCardsProcessor> cardProcessors) {
            CardProcessors = cardProcessors;
        }
        public WeightedCardsPicker() {
            CardProcessors = new List<WeightedCardsProcessor> {
                    new StatsCardsProcessesor(2f),
                    new ThemeCardsProcessesor(1f),
                    new RarityCardsProcessesor(6f),
                    new CurseWeightedCardProcessor(1f)
            };
        }

        public List<CardInfo> PickCard(List<CardInfo> cards) {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == CardChoice.instance.pickrID);
            WeightedCardsProcessor.WeightedCard[] weightedCards = cards
                .Select(c => new WeightedCardsProcessor.WeightedCard(1, c))
                .ToArray();

            foreach (var processor in CardProcessors) {
                WeightedCardsProcessor.WeightedCard[] processorWeightedCards = 
                    processor.GetCardsWeight(cards.ToArray(), player);

                for (int i = 0; i < processorWeightedCards.Length; i++) {
                    weightedCards[i].Weight *= processorWeightedCards[i].Weight + 1;
                }
            }

            foreach (var weightedCard in weightedCards) {
                LoggerUtils.Log(
                    LogLevel.Info, 
                    $"Card '{weightedCard.Card.cardName}' has weight: {weightedCard.Weight}"
                );
            }

            Array.Sort(
                weightedCards,
                (a, b) => b.Weight.CompareTo(a.Weight)
            );

            if (weightedCards.Length > 0) {
                return new List<CardInfo> {
                    weightedCards[0].Card
                };
            }

            return new List<CardInfo>();
        }
    }
}
