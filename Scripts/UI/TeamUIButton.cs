using RoundsWithBots.Patches;
using System.Linq;
using UnboundLib.Extensions;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.UI;

namespace RoundsWithBots.UI {
    public class TeamUIButton : MonoBehaviour {
        [Header("References")]
        [SerializeField] private BotMenuUIHandler BotMenuUIHandler;
        [SerializeField] private Graphic ColorGraphic;
        [SerializeField] private Graphic SelectionGraphic;
        [SerializeField] private Button ChangeTeamButton;

        [Header("Selection Colors")]
        public Color SelectionColor = Color.white * 0.8f;
        public Color DeselectionColor = Color.white * 0.2f;

        [Header("Colors")]
        public int ColorId = 0;
        public float LockedColorMult = 0.6f;

        private void Start() {
            SetSelectionColor();
            SetTeamColor();

            BotMenuUIHandler.OnTeamChange.AddListener(SetSelectionColor);
            ChangeTeamButton.onClick.AddListener(ChangeTeam);

            BotMenuUIHandler.OnPlayerTeamChange.AddListener(SetTeamColor);
        }

        private void OnDestroy() {
            BotMenuUIHandler.OnPlayerTeamChange.RemoveListener(SetTeamColor);
        }


        private void SetSelectionColor() {
            if(PlayerExtensions.GetAdditionalData(BotMenuUIHandler.CharacterSelectionInstance.currentPlayer).colorID == ColorId) {
                SelectionGraphic.color = SelectionColor;
            } else {
                SelectionGraphic.color = DeselectionColor;
            }
        }

        private void ChangeTeam() {
            BotMenuUIHandler.CharacterSelectionInstance.currentPlayer.AssignColorID(ColorId);
            BotMenuUIHandler.CharacterSelectionInstance.currentPlayer.SetColors();
            CharacterSelectionInstancePatch.RequestColorChange[BotMenuUIHandler.CharacterSelectionInstance] = true;
            BotMenuUIHandler.OnTeamChange?.Invoke();
        }

        private void SetTeamColor() {
            PlayerSkin skin = PlayerSkinBank.GetPlayerSkinColors(ColorId);
            Player[] otherPlayers = PlayerManager.instance.players
                .Where(p => p != null && p != BotMenuUIHandler.CharacterSelectionInstance.currentPlayer)
                .ToArray();

            bool colorAlreadyTaken = otherPlayers.Any(p => p.colorID() == ColorId);
            if (colorAlreadyTaken && !GameModeManager.CurrentHandler.AllowTeams) {
                ColorGraphic.color = skin.color * LockedColorMult;
                ChangeTeamButton.interactable = false;
            } else {
                ColorGraphic.color = skin.color;
                ChangeTeamButton.interactable = true;
            }
        }
    }
}
