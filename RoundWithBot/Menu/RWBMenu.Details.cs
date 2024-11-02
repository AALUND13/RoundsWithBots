using BepInEx.Configuration;
using System;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal static partial class RWBMenu {
        /* Bot Details Options Menu
         *
         * Title: Rounds With Bots - Bot Details
         *
         * Randomize Bot Faces: Toggle
         * - Description: Enable or disable randomization of bot faces.
         *
         * Selected Bot Face: Slider (0-7)
         * - Description: Select a specific bot face when Randomize Bot Faces is disabled.
         *
         */

        public static GameObject SelectedFaceObject;

        public static ConfigEntry<bool> RandomizationFace;
        public static ConfigEntry<int> SelectedFace;

        private static void CreateDetailsMenu(GameObject mainMenu) {
            GameObject facesMenu = MenuHandler.CreateMenu("Bot Details", () => { }, mainMenu, 40, parentForMenu: mainMenu.transform.parent.gameObject);

            MenuHandler.CreateText("<b>Rounds With Bots | Bot Details", facesMenu, out TextMeshProUGUI _, 70);
            AddBlank(facesMenu, 50);

            MenuHandler.CreateToggle(RandomizationFace.Value, "Randomize Bot Faces", facesMenu, (value) => { 
                RandomizationFace.Value = value;
                SelectedFaceObject.SetActive(!value);
            }, 30);

            AddBlank(facesMenu, 20);

            SelectedFaceObject = MenuHandler.CreateSlider("Selected Bot Face", facesMenu, 30, 0, 7, SelectedFace.Value, value => SelectedFace.Value = (int)Math.Round(value), out UnityEngine.UI.Slider _, true);
            SelectedFaceObject.SetActive(!RandomizationFace.Value);
        }
    }
}
