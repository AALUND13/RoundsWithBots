using System.Linq;
using UnboundLib.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace RoundsWithBots.UI {
    internal class FaceUIButton : MonoBehaviour {
        [Header("References")]
        [SerializeField] private BotMenuUIHandler BotMenuUIHandler;
        [SerializeField] private CharacterCreatorItemEquipper CharacterCreatorItemEquipper;
        [SerializeField] private Graphic ColorGraphic;
        [SerializeField] private Graphic SelectionGraphic;
        [SerializeField] private Button ChangeFaceButton;

        [Header("Selection Colors")]
        public Color SelectionColor = Color.white * 0.8f;
        public Color DeselectionColor = Color.white * 0.2f;

        [Header("Color Id")]
        public int FaceId = 0;


        private void Start() {
            GameObject[] faceSelectorArray = BotMenuUIHandler.CharacterSelectionInstance.transform.GetChild(0).GetChild(0).GetComponentsInChildren<HoverEvent>(true)
                                            .Select(x => x.gameObject)
                                            .ToArray();

            CharacterCreatorItemEquipper menuItemEquipper = faceSelectorArray[0].GetComponent<CharacterCreatorItemEquipper>();
            CharacterCreatorItemEquipper.defaultEyes = menuItemEquipper.defaultEyes;
            CharacterCreatorItemEquipper.defaultMouth = menuItemEquipper.defaultMouth;
            CharacterCreatorItemEquipper.EquipFace(CharacterCreatorHandler.instance.GetFacePreset(FaceId));

            SetSelectionColor();
            SetFaceColor();

            BotMenuUIHandler.OnFaceChange.AddListener(SetSelectionColor);
            BotMenuUIHandler.OnTeamChange.AddListener(SetFaceColor);
            ChangeFaceButton.onClick.AddListener(ChangeFace);
        }

        private void SetSelectionColor() {
            if (BotMenuUIHandler.CharacterSelectionInstance.currentlySelectedFace == FaceId) {
                SelectionGraphic.color = SelectionColor;
            } else {
                SelectionGraphic.color = DeselectionColor;
            }
        }

        private void SetFaceColor() {
            int colorId = PlayerExtensions.GetAdditionalData(BotMenuUIHandler.CharacterSelectionInstance.currentPlayer).colorID;
            PlayerSkin skin = PlayerSkinBank.GetPlayerSkinColors(colorId);

            ColorGraphic.color = skin.color;
        }

        private void ChangeFace() {
            BotMenuUIHandler.CharacterSelectionInstance.currentlySelectedFace = FaceId;
            BotMenuUIHandler.OnFaceChange?.Invoke();
        }
    }
}
