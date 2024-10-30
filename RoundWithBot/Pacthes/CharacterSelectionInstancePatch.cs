using HarmonyLib;
using InControl;
using UnboundLib;
using UnityEngine;

namespace RoundsWithBots.Pacthes {
    [HarmonyPatch(typeof(CharacterSelectionInstance))]
    internal class CharacterSelectionInstancePatch {
        [HarmonyPatch("StartPicking")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        public static void Postfix(CharacterSelectionInstance __instance, Player pickingPlayer) {
            MonoBehaviour playerAI = pickingPlayer.GetComponentInChildren<PlayerAI>() ?? (MonoBehaviour)pickingPlayer.GetComponentInChildren<PlayerAIZorro>();
            if(playerAI != null) {
                playerAI.gameObject.AddComponent<PlayerAIPhilip>();
                Object.Destroy(playerAI);
            }

            if(pickingPlayer.GetComponent<PlayerAPI>().enabled) {
                if(ConfigHandler.RandomizationFace.Value) {
                    __instance.currentlySelectedFace = Random.Range(0, 7);
                } else {
                    __instance.currentlySelectedFace = ConfigHandler.SelectedFace.Value;
                }
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        public static bool Prefix(CharacterSelectionInstance __instance) {
            if(__instance.currentPlayer == null) {
                return false;
            }


            if(__instance.currentPlayer.GetComponent<PlayerAPI>().enabled) {
                __instance.currentPlayer.data.playerVel.SetFieldValue("simulated", false);
                if(__instance.currentPlayer.data.playerActions == null) {
                    __instance.currentPlayer.data.playerActions = new PlayerActions();
                    __instance.currentPlayer.data.playerActions.Device = InputDevice.Null;
                }

                if(Input.GetKeyDown(KeyCode.R)) {
                    AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                    return false;
                }
            }
            return true;
        }
    }
}
