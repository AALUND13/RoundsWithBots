using TMPro;
using UnityEngine;

namespace RoundsWithBots.UI {
    public class FaceSelectorUI : MonoBehaviour {
        internal CharacterSelectionInstance characterSelectionInstance;
        public TextMeshProUGUI faceText;

        public void SetupForCharacterSelection(CharacterSelectionInstance characterSelectionInstance) {
            this.characterSelectionInstance = characterSelectionInstance;
            UpdateText(characterSelectionInstance.currentlySelectedFace);
        }

        public void IncrementFaceIndex() {
            characterSelectionInstance.currentlySelectedFace = (characterSelectionInstance.currentlySelectedFace + 1 + 8) % 8;

            UpdateText(characterSelectionInstance.currentlySelectedFace);
        }

        public void DecrementFaceIndex() {
            characterSelectionInstance.currentlySelectedFace = (characterSelectionInstance.currentlySelectedFace - 1 + 8) % 8;
            UpdateText(characterSelectionInstance.currentlySelectedFace);
        }

        private void UpdateText(int selectedFace) {
            faceText.text = $"FACE: {selectedFace}";
        }
    }
}
