using System.Collections;
using System.Collections.Generic;

namespace RoundsWithBots.Utils.CardPickerAIs {
    public interface ICardPickerAI {
        /// <summary>
        /// Method to pick a card from the list of cards.
        /// </summary>
        /// <param name="cards">The list of cards to pick from.</param>
        List<CardInfo> PickCard(List<CardInfo> cards);
    }
}
