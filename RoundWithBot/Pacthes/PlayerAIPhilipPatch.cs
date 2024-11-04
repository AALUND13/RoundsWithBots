using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RoundsWithBots.Patches {
    [HarmonyPatch(typeof(PlayerAIPhilip))]
    internal class PlayerAIPhilipPatch {
        private const float maxDistance = 1.0f;

        // This patch makes the AI use the shield when it is near the boundaries of the map.
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(PlayerAIPhilip __instance) {
            GeneralInput input = (GeneralInput)AccessTools.Field(typeof(PlayerAPI), "input").GetValue(__instance.GetComponentInParent<PlayerAPI>());

            OutOfBoundsHandler outOfBoundsHandlerInstance = GameObject.FindObjectOfType<OutOfBoundsHandler>();
            Vector3 bound = (Vector3)AccessTools.Method(typeof(OutOfBoundsHandler), "GetPoint")
                .Invoke(outOfBoundsHandlerInstance, new object[] { __instance.gameObject.transform.position });

            float diffX = Mathf.Abs(__instance.gameObject.transform.position.x - bound.x);
            float diffY = Mathf.Abs(__instance.gameObject.transform.position.y - bound.y);
            bool isNearBoundaries = (diffX <= maxDistance || diffY <= maxDistance) && (diffX >= maxDistance || diffY >= maxDistance);

            if(isNearBoundaries) {
                input.shieldWasPressed = true;
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
