using System.Collections.Generic;
using UnboundLib.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoundsWithBots.UI {
    internal class BotMenuUIHandler : MonoBehaviour {
        public static GameObject Prefab = RoundsWithBots.Instance?.Assets?.LoadAsset<GameObject>("Bot Menu");
        public static BotMenuUIHandler CurrentActiveInstance;
        public static UnityEvent OnPlayerTeamChange = new UnityEvent();

        [Header("References")]
        public GameObject MenuObject;
        public List<Graphic> TeamColorGraphics = new List<Graphic>();

        [HideInInspector] public CharacterSelectionInstance CharacterSelectionInstance { get; private set; }
        [HideInInspector] public UnityEvent OnTeamChange = new UnityEvent();
        [HideInInspector] public UnityEvent OnFaceChange = new UnityEvent();

        private bool isOpened = false;


        internal static BotMenuUIHandler CreateInstance(CharacterSelectionInstance characterSelection) {
            GameObject instanceCanvas = Instantiate(Prefab);
            BotMenuUIHandler menu = instanceCanvas.GetComponent<BotMenuUIHandler>();

            menu.CharacterSelectionInstance = characterSelection;
            menu.MenuObject.SetActive(false);

            return menu;
        }


        private void Start() {
            SetTeamColors();

            OnTeamChange.AddListener(SetTeamColors);
            OnTeamChange.AddListener(() => OnPlayerTeamChange?.Invoke());
            OnPlayerTeamChange?.Invoke();
        }

        private void Update() {
            if ((CharacterSelectionInstance == null || CharacterSelectionInstance.currentPlayer == null) && isOpened) {
                HideMenu();
            }

            if (CharacterSelectionInstance != null) {
                Vector3 s = CharacterSelectionInstance.transform.localScale;
                float rx = s.x == 0f ? 1f : 1f / s.x;
                float ry = s.y == 0f ? 1f : 1f / s.y;
                float rz = s.z == 0f ? 1f : 1f / s.z;
                GetComponent<RectTransform>().localScale = new Vector3(rx, ry, rz);
            }
        }


        public void OpenMenu() {
            if (CurrentActiveInstance != null && CurrentActiveInstance != this) {
                CurrentActiveInstance.HideMenu();
            }

            CurrentActiveInstance = this;
            isOpened = true;

            MenuObject.SetActive(true);
        }

        public void HideMenu() {
            if (!isOpened) return;
            isOpened = false;

            if (CurrentActiveInstance == this) {
                CurrentActiveInstance = null;
            }

            MenuObject.SetActive(false);
        }


        public void Refresh() {
            OnFaceChange?.Invoke();
            OnTeamChange?.Invoke();
        }


        public void RemovePlayer() {
            PlayerManager.instance.RemovePlayer(CharacterSelectionInstance.currentPlayer);
            OnPlayerTeamChange?.Invoke();
            HideMenu();
        }


        private void SetTeamColors() {
            foreach (var graphic in TeamColorGraphics) {
                int colorId = PlayerExtensions.GetAdditionalData(CharacterSelectionInstance.currentPlayer).colorID;
                PlayerSkin skin = PlayerSkinBank.GetPlayerSkinColors(colorId);

                graphic.color = skin.color;
            }
        }
    }
}
