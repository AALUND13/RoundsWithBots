using HarmonyLib;
using UnityEngine;

namespace RoundsWithBots.Pacthes {
    [HarmonyPatch(typeof(PlayerAIPhilip))]
    internal class PlayerAiPatch {
        private const float maxDistance = 1.0f;

        [HarmonyPatch("Update")]
        public  static void Postfix(PlayerAIPhilip __instance) {
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
    }
}
