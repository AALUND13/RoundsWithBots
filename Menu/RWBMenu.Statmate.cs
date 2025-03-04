using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal static partial class RWBMenu {
        /* Stalemate Options Menu
         * 
         * Title: Rounds With Bots - Stalemate Options
         * 
         * Stalemate Timer: Slider (0-60 seconds)
         * - Description: When a player dies, this timer starts counting down. When it reaches 0, the game begins dealing damage to the bots.
         * 
         * Stalemate Damage Cooldown: Slider (0-60 seconds)
         * - Description: The interval between each damage tick dealt to the bots during a stalemate.
         * 
         * Stalemate Damage Duration: Slider (0-60 seconds)
         * - Description: The duration over which damage is dealt to the bots during each damage tick.
         * 
         */

        public static ConfigEntry<float> StalemateTimer;
        public static ConfigEntry<float> StalemateDamageCooldown;
        public static ConfigEntry<float> StalemateDamageDuration;

        private static void CreateStalemateMenu(GameObject mainMenu) {
            GameObject stalemateMenu = MenuHandler.CreateMenu("Stalemate Options", () => { }, mainMenu, 40, parentForMenu: mainMenu.transform.parent.gameObject);

            MenuHandler.CreateText("<b>Rounds With Bots | Stalemate Options", stalemateMenu, out TextMeshProUGUI _, 70);
            AddBlank(stalemateMenu, 50);

            MenuHandler.CreateSlider("Stalemate Timer", stalemateMenu, 30, 0, 60, StalemateTimer.Value, value => StalemateTimer.Value = value, out UnityEngine.UI.Slider _);
            AddBlank(stalemateMenu, 20);

            MenuHandler.CreateSlider("Stalemate Damage Cooldown", stalemateMenu, 30, 0, 60, StalemateDamageCooldown.Value, value => StalemateDamageCooldown.Value = value, out UnityEngine.UI.Slider _);
            AddBlank(stalemateMenu, 20);

            MenuHandler.CreateSlider("Stalemate Damage Duration", stalemateMenu, 30, 0, 60, StalemateDamageDuration.Value, value => StalemateDamageDuration.Value = value, out UnityEngine.UI.Slider _);
        }
    }
}
