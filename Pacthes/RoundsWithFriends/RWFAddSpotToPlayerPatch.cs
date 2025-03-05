using HarmonyLib;
using RWF.UI;

namespace RoundsWithBots.Patches.RWF {
    [HarmonyPatch(typeof(PlayerSpotlight))]
    internal class RWFAddSpotToPlayerPatch {
        [HarmonyPatch("AddSpotToPlayer")]
        public static bool Prefix(Player player) {

            return !player.GetComponent<PlayerAPI>().enabled;
        }
    }
}
