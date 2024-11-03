using BepInEx.Configuration;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal static partial class RWBMenu {
        /* RWB Menu
         * 
         * Title: Rounds With Bots
         * 
         * Bot Faces: Submenu
         * - Description: Configure the appearance of bot faces.
         *
         * Stalemate Options: Submenu
         * - Description: Configure stalemate-related settings.
         *
         * Cards Picker: Submenu
         * - Description: Configure the card picker for bots.
         *
         * Debug Mode: Toggle
         * - Description: Enable or disable debug mode for additional logging and debugging features.
         *
         */

        public static ConfigEntry<bool> DebugMode;

        public static void RegisterMenu(string modName, ConfigFile config) {
            Unbound.RegisterMenu(modName, () => { }, CreateRWBMenu, null, false);

            DebugMode = config.Bind(modName, "DebugMode", false, "Enable or disable debug mode for additional logging and debugging features.");

            RandomizationFace = config.Bind(modName, "RandomizationFace", true, "Enable or disable randomization of bot faces.");
            SelectedFace = config.Bind(modName, "SelectedFace", 0, "Select a specific bot face when Randomize Bot Faces is disabled.");

            StalemateTimer = config.Bind(modName, "StalemateTimer", 10f, "The time in seconds before a stalemate is declared.");
            StalemateDamageCooldown = config.Bind(modName, "StalemateDamageCooldown", 1f, "The time in seconds before a player can take damage again after a stalemate.");
            StalemateDamageDuration = config.Bind(modName, "StalemateDamageDuration", 10f, "The time in seconds that a player takes damage after a stalemate.");

            CycleDelay = config.Bind(modName, "CycleDelay", 0.3f, "The delay between cycling through cards.");
            PreCycleDelay = config.Bind(modName, "PreCycleDelay", 1f, "The delay before cycling through cards.");
            GoToCardDelay = config.Bind(modName, "GoToCardDelay", 0.2f, "The delay between going to a specific card.");
            PickDelay = config.Bind(modName, "PickDelay", 0.5f, "The delay before picking a card.");
        }

        private static void CreateRWBMenu(GameObject mainMenu) {
            MenuHandler.CreateText("<b>Rounds With Bots", mainMenu, out TextMeshProUGUI _, 70);
            AddBlank(mainMenu, 50);

            CreateDetailsMenu(mainMenu);
            AddBlank(mainMenu, 20);

            CreateStalemateMenu(mainMenu);
            AddBlank(mainMenu, 20);

            CreateCardsPickerMenu(mainMenu);
            AddBlank(mainMenu, 20);

            MenuHandler.CreateToggle(DebugMode.Value, "<#c41010>Debug Mode", mainMenu, value => DebugMode.Value = value, 30);
        }

        private static void AddBlank(GameObject menu, int size = 30) {
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, size);
        }
    }
}
