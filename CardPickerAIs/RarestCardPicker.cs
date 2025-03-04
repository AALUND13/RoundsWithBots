using System.Collections.Generic;
using System.Linq;

namespace RoundsWithBots.CardPickerAIs {
    public class RarestCardPicker : ICardPickerAI {
        public List<CardInfo> PickCard(List<CardInfo> cards) {
            var rarestCards = cards
                .GroupBy(card => card.rarity)
                .OrderBy(group => group.Key)
                .LastOrDefault();

            return rarestCards?.ToList();
        }
    }
}
