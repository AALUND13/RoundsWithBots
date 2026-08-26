using BepInEx.Logging;
using HarmonyLib;
using RoundsWithBots.Extensions;
using RoundsWithBots.Utils.Debug;
using RWF;
using RWF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace RoundsWithBots.Pacthes.RWF {
    [HarmonyPatch(typeof(RoundEndHandler), "OnRoundEnd", MethodType.Enumerator)]
    public class RoundEndHandlerPatch {
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        ) {
            try {
                LoggerUtils.Log(LogLevel.Debug, "RoundEndHandlerPatch.Transpiler called");

                var codes = new List<CodeInstruction>(instructions);
                LoggerUtils.Log(LogLevel.Debug, $"Transpiler received {codes.Count} instructions");

                CodeMatcher codeMatcher = new CodeMatcher(instructions, il);
                LoggerUtils.Log(LogLevel.Debug, "Created CodeMatcher - attempting MatchForward for PopUpMenu.Open");
                codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PopUpMenu), "Open", new Type[] {
                        typeof(List<string>),
                        typeof(Action<string>)
                    })),
                    new CodeMatch(OpCodes.Br)
                ).Advance(1);

                Label targetLabel = (Label)codeMatcher.Instruction.operand;
                LoggerUtils.Log(LogLevel.Debug, "Matched PopUpMenu.Open and Br_S; extracted target label");

                codeMatcher.Advance(1).Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RoundEndHandlerPatch), nameof(Rematch))),
                    new CodeInstruction(OpCodes.Br_S, targetLabel)
                ).CreateLabel(out Label botsRoundsEndLabel);
                LoggerUtils.Log(LogLevel.Debug, "Inserted Rematch call and branch; created botsRoundsEndLabel");

                codeMatcher.MatchBack(false,
                    new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(List<string>), Type.EmptyTypes))
                );
                List<Label> originalLabels = new List<Label>(codeMatcher.Instruction.labels);


                codeMatcher.Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RoundEndHandlerPatch), nameof(ShouldDoBotsRematch))) { labels = originalLabels },
                    new CodeInstruction(OpCodes.Brtrue_S, botsRoundsEndLabel)
                );
                LoggerUtils.Log(LogLevel.Debug, "Inserted ShouldDoBotsRematch check before List<string> construction");

                var result = codeMatcher.Instructions().ToList();
                LoggerUtils.Log(LogLevel.Debug, $"Transpiler finished, returning {result.Count} instructions");
                return result;
            } catch (Exception e) {
                LoggerUtils.LogError($"RoundEndHandlerPatch.Transpiler failed: {e}");
                return instructions;
            }
        }

        private static bool ShouldDoBotsRematch() {
            bool isAllBot = PlayerManager.instance.players.All(p => p.data.GetAdditionalData().IsBot
                || ModdingUtils.AIMinion.Extensions.CharacterDataExtension.GetAdditionalData(p.data).isAIMinion);

            return isAllBot;
        }

        private static void Rematch() {
            RoundEndHandler.instance.OnGameOverChoose("REMATCH");
        }
    }
}
