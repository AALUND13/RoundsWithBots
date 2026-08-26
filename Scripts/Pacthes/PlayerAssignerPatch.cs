using BepInEx.Logging;
using HarmonyLib;
using InControl;
using ModdingUtils.AIMinion;
using RoundsWithBots.AI;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace RoundsWithBots.Patches {
    [HarmonyPatch(typeof(PlayerAssigner))]
    internal class PlayerAssignerPatch {
        [HarmonyPatch("CreatePlayer")]
        public static bool Prefix(bool isAI, ref IEnumerator __result) {
            if (GameManager.instance.isPlaying && !AIMinionHandler.sandbox) {
                __result = EmptyEnumerator();
                return false;
            }
            return true;
        }

        [HarmonyPatch("CreatePlayer", MethodType.Enumerator)]
        [HarmonyDebug]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        ) {
            LoggerUtils.Log(LogLevel.Debug, "PlayerAssignerPatch.Transpiler called");

            var codes = new List<CodeInstruction>(instructions);
            LoggerUtils.Log(LogLevel.Debug, $"Transpiler received {codes.Count} instructions");

            CodeMatcher codeMatcher = new CodeMatcher(instructions, il);
            LoggerUtils.Log(LogLevel.Debug, "Created CodeMatcher - attempting MatchForward for PlayerAssigner.CreatePlayeer");
            codeMatcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Component), nameof(Component.GetComponent), Type.EmptyTypes).MakeGenericMethod(typeof(CharacterData))),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CharacterData), nameof(CharacterData.SetAI)))
            ).MatchForward(false,
                new CodeMatch(OpCodes.Pop)
            ).Insert(
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerAssignerPatch), nameof(CreateAdvacnedAI)))
            );
            LoggerUtils.Log(LogLevel.Debug, "Inserted CreateAdvacnedAI call");


            var result = codeMatcher.Instructions().ToList();
            LoggerUtils.Log(LogLevel.Debug, $"Transpiler finished, returning {result.Count} instructions");
            return result;
        }

        private static void CreateAdvacnedAI(CharacterData data) {
            PlayerAI playerAI = data.GetComponentInChildren<PlayerAI>();
            PlayerAIZorro zorroAI = data.GetComponentInChildren<PlayerAIZorro>();

            if (playerAI != null) {
                playerAI.gameObject.AddComponent<PlayerAIBase>();
                Component.Destroy(playerAI);
            } else if (zorroAI != null) {
                zorroAI.gameObject.AddComponent<PlayerAIBase>();
                data.healthHandler.delayedReviveAction -= zorroAI.Init;
                Component.Destroy(zorroAI);
            }

            data.playerActions = new PlayerActions();
            data.playerActions.Device = InputDevice.Null;

            data.GetAdditionalData().IsBot = true;
            LoggerUtils.Log(LogLevel.Debug, "Created advanced bot");
        }

        private static IEnumerator EmptyEnumerator() {
            yield break;
        }
    }
}
