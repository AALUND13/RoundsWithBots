using InControl;
using RoundsWithBots.Patches;
using RWF;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Extensions;
using UnboundLib.GameModes;
using UnboundLib.Utils;
using UnityEngine;

namespace RoundsWithBots.UI {
    public class TeamSelectorUI : MonoBehaviour {
        internal CharacterSelectionInstance characterSelectionInstance;
        public TextMeshProUGUI teamText;

        public void SetupForCharacterSelection(CharacterSelectionInstance characterSelectionInstance) {
            this.characterSelectionInstance = characterSelectionInstance;
            UpdateText(UnboundLib.Extensions.PlayerExtensions.GetAdditionalData(characterSelectionInstance.currentPlayer).colorID);
        }

        public void IncrementTeamIndex() {
            int selectedColor = ChanageTeam(1);
            characterSelectionInstance.currentPlayer.AssignColorID(selectedColor);
            characterSelectionInstance.currentPlayer.SetColors();
            CharacterSelectionInstancePatch.RequestColorChange[characterSelectionInstance] = true;
            UpdateText(selectedColor);
        }

        public void DecrementTeamIndex() {
            int selectedColor = ChanageTeam(-1);
            characterSelectionInstance.currentPlayer.AssignColorID(selectedColor);
            characterSelectionInstance.currentPlayer.SetColors();
            CharacterSelectionInstancePatch.RequestColorChange[characterSelectionInstance] = true;
            UpdateText(selectedColor);
        }

        public int ChanageTeam(int colorIDDelta) {
            int selectedColor = (UnboundLib.Extensions.PlayerExtensions.GetAdditionalData(characterSelectionInstance.currentPlayer).colorID + colorIDDelta + RWFMod.MaxColorsHardLimit) % RWFMod.MaxColorsHardLimit;

            if(!GameModeManager.CurrentHandler.AllowTeams) {
                int orig = characterSelectionInstance.currentPlayer.colorID();

                while(PlayerManager.instance.players.Select(p => p.colorID()).Contains(selectedColor) && selectedColor != orig) {
                    selectedColor = (selectedColor + colorIDDelta + RWFMod.MaxColorsHardLimit) % RWFMod.MaxColorsHardLimit;
                }
            }

            return selectedColor;
        }

        private void UpdateText(int selectedColor) {
            PlayerSkin skin = PlayerSkinBank.GetPlayerSkinColors(selectedColor);

            teamText.text = $"TEAM: <color=#{ColorUtility.ToHtmlStringRGB(skin.color * 1.5f)}>{ExtraPlayerSkins.GetTeamColorName(selectedColor).ToUpper()}";
        }

        private static void SimulatePress(PlayerAction action) {
            action.CommitWithValue(1, (ulong)action.GetFieldValue("pendingTick"), 0);
        }
    }
}
