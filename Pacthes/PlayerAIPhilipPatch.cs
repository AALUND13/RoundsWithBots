using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RoundsWithBots.Patches {
    [HarmonyPatch(typeof(PlayerAIPhilip))]
    internal class PlayerAIPhilipPatch {
        private const float maxDistance = 1.0f;
        public static List<Collider2D> DamageBoxesColliders = new List<Collider2D>();

        // This patch makes the AI use the shield when it is near the boundaries of the map or near a damage box.
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(PlayerAIPhilip __instance) {
            GeneralInput input = (GeneralInput)AccessTools.Field(typeof(PlayerAPI), "input").GetValue(__instance.GetComponentInParent<PlayerAPI>());

            OutOfBoundsHandler outOfBoundsHandlerInstance = GameObject.FindObjectOfType<OutOfBoundsHandler>();
            Vector3 bound = (Vector3)AccessTools.Method(typeof(OutOfBoundsHandler), "GetPoint")
                .Invoke(outOfBoundsHandlerInstance, new object[] { __instance.transform.position });

            float diffX = Mathf.Abs(__instance.transform.position.x - bound.x);
            float diffY = Mathf.Abs(__instance.transform.position.y - bound.y);
            bool isNearBoundaries = (diffX <= maxDistance || diffY <= maxDistance) && (diffX >= maxDistance || diffY >= maxDistance);

            if(isNearBoundaries) {
                input.shieldWasPressed = true;
            }

            foreach(var damageBoxCollider in DamageBoxesColliders) {
                if(damageBoxCollider == null) continue;

                Vector2 closestPoint = damageBoxCollider.bounds.ClosestPoint(__instance.transform.position);
                float distance = Vector2.Distance(closestPoint, __instance.transform.position);

                if(distance <= maxDistance) {
                    input.shieldWasPressed = true;
                    break;
                }
            }
        }

        // This patch allows the AI to detect players even when they are behind background objects by ignoring the "BackgroundObject" layer.
        [HarmonyTranspiler]
        [HarmonyPatch("CanSee")]
        public static IEnumerable<CodeInstruction> CanSeeTranspiler(IEnumerable<CodeInstruction> instructions) {
            return ApplyLayerMaskToRaycast(instructions);
        }

        // This patch prevents the AI from getting stuck in the "BackgroundObject" objects by ignoring the "BackgroundObject" layer during ground checks.
        [HarmonyTranspiler]
        [HarmonyPatch("CheckGround")]
        public static IEnumerable<CodeInstruction> CheckGroundTranspiler(IEnumerable<CodeInstruction> instructions) {
            return ApplyLayerMaskToRaycast(instructions);
        }

        private static IEnumerable<CodeInstruction> ApplyLayerMaskToRaycast(IEnumerable<CodeInstruction> instructions) {
            var raycastMethod = AccessTools.Method(typeof(Physics2D), nameof(Physics2D.Raycast), new[] {
                typeof(Vector2), typeof(Vector2), typeof(float), typeof(int)
            });

            int layerToIgnore = LayerMask.NameToLayer("BackgroundObject");
            int layerMask = ~(1 << layerToIgnore); // Create a bitmask that ignores the BackgroundObject layer

            foreach(var code in instructions) {
                if(code.opcode == OpCodes.Call && code.operand is MethodInfo methodInfo && methodInfo.Name == "Raycast") {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, layerMask);
                    yield return new CodeInstruction(OpCodes.Call, raycastMethod);
                } else {
                    yield return code;
                }
            }
        }
    }
}
