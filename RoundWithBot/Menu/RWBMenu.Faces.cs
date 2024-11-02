using BepInEx.Configuration;
using System;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal static partial class RWBMenu {
        /* Faces Options Menu
         *
         * Title: Rounds With Bots - Bot Faces
         *
         * Randomize Bot Faces: Toggle
         * - Description: Enable or disable randomization of bot faces.
         *
         * Selected Bot Face: Slider (0-7)
         * - Description: Select a specific bot face when Randomize Bot Faces is disabled.
         *
         */

        public static ConfigEntry<bool> RandomizationFace;
        public static ConfigEntry<int> SelectedFace;

        private static void CreateFacesMenu(GameObject mainMenu) {
            GameObject facesMenu = MenuHandler.CreateMenu("Bot Faces", () => { }, mainMenu, 40, parentForMenu: mainMenu.transform.parent.gameObject);

            MenuHandler.CreateText("Rounds With Bots | Bot Faces", facesMenu, out TextMeshProUGUI _, 70);
            AddBlank(facesMenu, 50);

            MenuHandler.CreateToggle(RandomizationFace.Value, "Randomize Bot Faces", facesMenu, value => RandomizationFace.Value = value, 30);

            AddBlank(facesMenu, 20);
            MenuHandler.CreateText("<#c41010>Options below only work when 'Randomize Bot Faces' is off!", facesMenu, out TextMeshProUGUI _, 40);
            AddBlank(facesMenu, 20);

            MenuHandler.CreateSlider("Selected Bot Face", facesMenu, 30, 0, 7, SelectedFace.Value, value => SelectedFace.Value = (int)Math.Round(value), out UnityEngine.UI.Slider _, true);
        }
    }
}
