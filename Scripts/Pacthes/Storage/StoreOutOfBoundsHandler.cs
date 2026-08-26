using HarmonyLib;
using System.Collections.Generic;
using UnboundLib;

namespace RoundsWithBots.Pacthes.Storage {
    [HarmonyPatch(typeof(OutOfBoundsHandler), "Start")]
    public class StoreOutOfBoundsHandler {
        public static Dictionary<Player, OutOfBoundsHandler> outOfBoundsHandlers =
            new Dictionary<Player, OutOfBoundsHandler>();

        private static void Postfix(OutOfBoundsHandler __instance) {
            outOfBoundsHandlers.Add(((CharacterData)__instance.GetFieldValue("data")).player, __instance);
        }
    }
}
