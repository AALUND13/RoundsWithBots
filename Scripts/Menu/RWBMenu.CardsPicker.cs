using BepInEx.Configuration;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal partial class RWBMenu {
        /* Cards Picker Menu
         * 
         * Title: Rounds With Bots - Cards Picker
         * 
         * Cycle Delay: Slider (0-1 seconds)
         * - Description: The delay between cycling through cards.
         * 
         * Pre-Cycle Delay: Slider (0-1 seconds)
         * - Description: The delay before cycling through cards.
         * 
         * Go To Card Delay: Slider (0-1 seconds)
         * - Description: The delay between going to a specific card.
         * 
         * Pick Delay: Slider (0-1 seconds)
         * - Description: The delay before picking a card.
         * 
         */

        public static ConfigEntry<float> CycleDelay;
        public static ConfigEntry<float> PreCycleDelay;
        public static ConfigEntry<float> GoToCardDelay;
        public static ConfigEntry<float> PickDelay;

        private static void CreateCardsPickerMenu(GameObject mainMenu) {
            GameObject cardsPickerMenu = MenuHandler.CreateMenu("Cards Picker", () => { }, mainMenu, 40, parentForMenu: mainMenu.transform.parent.gameObject);

            MenuHandler.CreateText("<b>Rounds With Bots | Cards Picker", cardsPickerMenu, out TextMeshProUGUI _, 70);
            AddBlank(cardsPickerMenu, 50);

            MenuHandler.CreateSlider("Cycle Delay", cardsPickerMenu, 30, 0, 1, CycleDelay.Value, value => CycleDelay.Value = value, out UnityEngine.UI.Slider _);
            AddBlank(cardsPickerMenu, 20);

            MenuHandler.CreateSlider("Pre-Cycle Delay", cardsPickerMenu, 30, 0, 5, PreCycleDelay.Value, value => PreCycleDelay.Value = value, out UnityEngine.UI.Slider _);
            AddBlank(cardsPickerMenu, 20);

            MenuHandler.CreateSlider("Go To Card Delay", cardsPickerMenu, 30, 0, 1, GoToCardDelay.Value, value => GoToCardDelay.Value = value, out UnityEngine.UI.Slider _);
            AddBlank(cardsPickerMenu, 20);

            MenuHandler.CreateSlider("Pick Delay", cardsPickerMenu, 30, 0, 5, PickDelay.Value, value => PickDelay.Value = value, out UnityEngine.UI.Slider _);
        }
    }
}
