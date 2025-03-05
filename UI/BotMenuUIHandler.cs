using RoundsWithBots.Utils;
using System.Collections;
using UnityEngine;

namespace RoundsWithBots.UI {
    internal class BotMenuUIHandler : MonoBehaviour {
        public static GameObject Prefab = RoundsWithBots.Instance.Assets?.LoadAsset<GameObject>("BotMenu");
        public static GameObject CurrentInstance;

        public GameObject MenuObject;

        public FaceSelectorUI faceSelectorUI;
        public TeamSelectorUI teamSelectorUI;

        private CharacterSelectionInstance characterSelectionInstance;

        private bool IsOpen => GetComponent<CanvasGroup>().alpha == 1;

        public void Update() {
            if ((characterSelectionInstance == null || characterSelectionInstance.currentPlayer == null) && IsOpen) {
                Hide();
            }
        }

        public static BotMenuUIHandler Show(CharacterSelectionInstance characterSelectionInstance) {
            if(CurrentInstance != null) {
                CurrentInstance.GetComponent<BotMenuUIHandler>().Hide();
            }
            CurrentInstance = CreateInstance(Prefab);
            

            BotMenuUIHandler instanceMenu = CurrentInstance.GetComponent<BotMenuUIHandler>();
            instanceMenu.characterSelectionInstance = characterSelectionInstance;
            instanceMenu.faceSelectorUI.SetupForCharacterSelection(characterSelectionInstance);
            instanceMenu.teamSelectorUI.SetupForCharacterSelection(characterSelectionInstance);

            instanceMenu.GetComponent<CanvasGroup>().interactable = true;
            instanceMenu.StartCoroutine(instanceMenu.ToggleMenu(true));

            return instanceMenu;
        }

        public void RemovePlayer() {
            PlayerManager.instance.RemovePlayer(characterSelectionInstance.currentPlayer);
        }

        public void Hide() {
            StartCoroutine(HideThenDestroy());
        }

        private IEnumerator HideThenDestroy() {
            GetComponent<CanvasGroup>().interactable = false;
            yield return ToggleMenu(false);

            Destroy(gameObject);
        }

        private IEnumerator ToggleMenu(bool toggle) {
            if (IsOpen == toggle) yield break;

            float min = toggle ? 0f : 1f;
            float max = toggle ? 1f : 0f;
            yield return EaseUtils.EaseCoroutine(0.2f, EaseUtils.EaseType.easeInQuad, (float value) => {
                GetComponent<CanvasGroup>().alpha = value;
            }, null, min, max);

            GetComponent<CanvasGroup>().blocksRaycasts = toggle;
        }

        private static GameObject CreateInstance(GameObject prefab) {
            GameObject instanceCanvas = Instantiate(prefab);
            DontDestroyOnLoad(instanceCanvas);

            GameObject instance = instanceCanvas.transform.GetChild(0).gameObject;
            instance.GetComponent<CanvasGroup>().interactable = false;
            instance.GetComponent<CanvasGroup>().blocksRaycasts = false;
            instance.GetComponent<CanvasGroup>().alpha = 0;

            return instance;
        }
    }
}
