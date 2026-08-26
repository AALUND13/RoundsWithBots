using BepInEx.Configuration;
using RoundsWithBots.Utils.Debug;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundsWithBots.Menu {
    internal static partial class RWBMenu {
        /* Bot Debug Options Menu
         *
         * Title: Rounds With Bots - Debugs Options
         *
         * Enable Map Scaner Visualizer: Toggle
         *
         * Enable Bots Pathfinding Visualizer: Toggle
         * 
         * Enable Debug Logging: Toggle
         * Enable Profiler: Toggle
         */

        public static ConfigEntry<bool> EnableMapScannerVisualizer;
        public static ConfigEntry<bool> EnableBotsPathfindingVisualizer;

        public static ConfigEntry<bool> EnableDebugLogging;
        public static ConfigEntry<bool> EnableProfiler;

        private static void CreateDebugMenu(GameObject mainMenu) {
            GameObject debugMenu = MenuHandler.CreateMenu("DEBUG Options", () => { }, mainMenu, 40, parentForMenu: mainMenu.transform.parent.gameObject);

            MenuHandler.CreateText("<b>Rounds With Bots | DEBUG Options", debugMenu, out TextMeshProUGUI _, 70);
            AddBlank(debugMenu, 50);

            MenuHandler.CreateToggle(EnableMapScannerVisualizer.Value, "Enable Map Scanner Visualizer", debugMenu, (value) => {
                EnableMapScannerVisualizer.Value = value;
            }, 30);
            MenuHandler.CreateToggle(EnableBotsPathfindingVisualizer.Value, "Enable Bots Pathfinding Visualizer", debugMenu, (value) => {
                EnableBotsPathfindingVisualizer.Value = value;
            }, 30);

            AddBlank(debugMenu, 20);

            MenuHandler.CreateToggle(EnableDebugLogging.Value, "Enable Debug Logging", debugMenu, (value) => {
                EnableDebugLogging.Value = value;
            }, 30);
            MenuHandler.CreateToggle(EnableProfiler.Value, "Enable Profiler", debugMenu, (value) => {
                EnableProfiler.Value = value;
            }, 30);
        }

        public static DebugVisualizer.VisualizerCategory GetEnableVisualizer() {
            DebugVisualizer.VisualizerCategory category = DebugVisualizer.VisualizerCategory.None;

            if (EnableMapScannerVisualizer.Value)
                category |= DebugVisualizer.VisualizerCategory.MapScanner;

            if (EnableBotsPathfindingVisualizer.Value)
                category |= DebugVisualizer.VisualizerCategory.Bots;

            return category;
        }
    }
}
