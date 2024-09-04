using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace RoundWithBot.Patches.RWF {
    [HarmonyPatch(typeof(CardChoice), "GetRandomCard")]
    public static class CardChoicePatch {
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions) {
            var isAExcludeCardMethod = AccessTools.Method(typeof(RWB.RoundWithBot), "IsAExcludeCard", new Type[] { typeof(GameObject) });
            var getRandomCardMethod = AccessTools.Method(typeof(CardChoice), "GetRandomCard");

            var instructionsList = instructions.ToList();
            var skipLabel = ilGenerator.DefineLabel();

            for(int i = 0; i < instructionsList.Count; i++) {
                var code = instructionsList[i];
                var forward = i + 1 < instructionsList.Count ? instructionsList[i + 1] : null;

                if(code.opcode == OpCodes.Ldloc_0 && forward != null && forward.opcode == OpCodes.Ret) {
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // Load the original card
                    yield return new CodeInstruction(OpCodes.Call, isAExcludeCardMethod); // Call IsAExcludeCard
                    yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel); // If not excluded, skip the replacement

                    yield return new CodeInstruction(OpCodes.Ldarg_0); // Load the CardChoice instance (this)
                    yield return new CodeInstruction(OpCodes.Call, getRandomCardMethod); // Recalculate the random card using instance method
                    yield return new CodeInstruction(OpCodes.Stloc_0); // Store the recalculated card in the local variable 0
                    yield return new CodeInstruction(OpCodes.Br_S, skipLabel);

                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(skipLabel);
                } else {
                    yield return code;
                }
            }

            UnityEngine.Debug.Log("Patched GetRandomCard");
        }
    }
}

