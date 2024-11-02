using HarmonyLib;
using InControl;
using ModdingUtils.AIMinion;
using System.Collections;

namespace RoundsWithBots.Pacthes {
    [HarmonyPatch(typeof(PlayerAssigner))]
    public class PlayerAssignerPatch {
        [HarmonyPatch("CreatePlayer")]
        public static bool Prefix(PlayerAssigner __instance, InputDevice inputDevice, bool isAI, ref IEnumerator __result) {
            if(isAI 
                && GameManager.instance.isPlaying 
                && !AIMinionHandler.sandbox
            ) {
                __result = EmptyEnumerator();
                return false;
            }
            return true;
        }

        private static IEnumerator EmptyEnumerator() {
            yield break;
        }
    }
}
