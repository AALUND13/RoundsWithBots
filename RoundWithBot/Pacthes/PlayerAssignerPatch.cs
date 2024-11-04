using HarmonyLib;
using InControl;
using ModdingUtils.AIMinion;
using RoundsWithBots.Extensions;
using System.Collections;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace RoundsWithBots.Pacthes {
    [HarmonyPatch(typeof(PlayerAssigner))]
    public class PlayerAssignerPatch {
        [HarmonyPatch("CreatePlayer")]
        public static bool Prefix(bool isAI, ref IEnumerator __result) {
            if(GameManager.instance.isPlaying && !AIMinionHandler.sandbox) {
                __result = EmptyEnumerator();
                return false;
            } else if (isAI) {
                RoundsWithBots.Instance.StartCoroutine(DelayedAIReplacement());
            }
            return true;
        }

        private static IEnumerator DelayedAIReplacement() {
            yield return null;

            Player player = PlayerManager.instance.players.Last();
            if(player == null) {
                Debug.LogError("Player could not be found.");
                yield break;
            }

            MonoBehaviour playerAI = player.GetComponentInChildren<PlayerAI>() ?? (MonoBehaviour)player.GetComponentInChildren<PlayerAIZorro>();
            if(playerAI != null) {
                playerAI.gameObject.AddComponent<PlayerAIPhilip>();
                player.GetComponentInParent<CharacterData>().GetAdditionalData().IsBot = true;

                Object.Destroy(playerAI);
            }
        }

        private static IEnumerator EmptyEnumerator() {
            yield break;
        }
    }
}
