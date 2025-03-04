using HarmonyLib;
using RWF;
using RWF.UI;
using UnityEngine;

namespace RoundsWithBots.Pacthes.RWF {
    [HarmonyPatch(typeof(KeybindHints))]
    internal class KeybindHintsPatch {
        [HarmonyPatch("CreateLocalHints")]
        public static void Postfix() {
            if(PlayerPrefs.GetInt(RWFMod.GetCustomPropertyKey("ShowKeybinds"), 1) != 0) {
                KeybindHints.AddHint("to ready up all bots", "[R]");
            }
        }
    }
}
