using HarmonyLib;
using RoundsWithBots.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoundsWithBots.Pacthes {
    [HarmonyPatch(typeof(Map))]
    internal class MapPatch {
        [HarmonyPatch("StartMatch")]
        public static void Postfix() {
            RoundsWithBots.Instance.StartCoroutine(SetDamageBoxesColliders());
        }

        public static IEnumerator SetDamageBoxesColliders() {
            yield return null;
            List<DamageBox> damageBoxes = GameObject.FindObjectsOfType<DamageBox>().ToList();
            PlayerAIPhilipPatch.DamageBoxesColliders.Clear();
            foreach(DamageBox damageBox in damageBoxes) {
                PlayerAIPhilipPatch.DamageBoxesColliders.Add(damageBox.GetComponent<Collider2D>());
            }

            yield break;
        }
    }
}
