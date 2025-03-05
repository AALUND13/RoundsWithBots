
using HarmonyLib;
using InControl;
using RoundsWithBots.Extensions;
using RoundsWithBots.Menu;
using RoundsWithBots.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Extensions;
using UnboundLib.GameModes;
using UnboundLib.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RoundsWithBots.Patches {
    [HarmonyPatch(typeof(CharacterSelectionInstance))]
    internal class CharacterSelectionInstancePatch {
        public static Dictionary<CharacterSelectionInstance, bool> RequestColorChange = new Dictionary<CharacterSelectionInstance, bool>();

        [HarmonyPatch("StartPicking")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        [HarmonyPostfix]
        public static void StartPickingPostfix(CharacterSelectionInstance __instance, Player pickingPlayer) {
            if(__instance.currentPlayer.GetComponent<PlayerAPI>().enabled) {
                if(RWBMenu.RandomizationFace.Value) {
                    __instance.currentlySelectedFace = Random.Range(0, 7);
                } else {
                    __instance.currentlySelectedFace = RWBMenu.SelectedFace.Value;
                }
            }

            __instance.ExecuteAfterFrames(1, () => {
                __instance.transform.GetChild(0).GetChild(1).gameObject.SetActive(false); // This line hides the arrow that the player can uses the buttons

                SetupButton(__instance);
            });
        }

        private static void SetupButton(CharacterSelectionInstance __instance) {
            GameObject[] faceSelectorArray = __instance.transform.GetChild(0).GetChild(0).GetComponentsInChildren<HoverEvent>(true)
                                            .Select(x => x.gameObject)
                                            .ToArray();

            foreach(GameObject faceSelector in faceSelectorArray) {
                if(__instance.currentPlayer.GetComponent<PlayerAPI>().enabled) {
                    GameObject tooltip = faceSelector.transform.Find("Tooltip").gameObject;
                    Button button = tooltip.GetOrAddComponent<Button>();

                    Image image = tooltip.GetComponentInChildren<Image>(true);
                    TextMeshProUGUI text = tooltip.GetComponentInChildren<TextMeshProUGUI>();
                    ControllerImageToggler controllerImageToggler = image.GetComponent<ControllerImageToggler>();

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        UnityEngine.Debug.Log("The bot have be clicked.");

                        Vector2 cursorPosition = Input.mousePosition;

                        BotMenuUIHandler botMenu = BotMenuUIHandler.Show(__instance);
                        RectTransform rect = botMenu.GetComponent<RectTransform>();
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, cursorPosition, null, out Vector2 localCursor);

                        localCursor.y -= rect.rect.height / 2;

                        botMenu.transform.position = rect.TransformPoint(localCursor);
                    });
                    button.targetGraphic = text;

                    image.overrideSprite = controllerImageToggler.MKSprite;
                    image.transform.localEulerAngles = new Vector3(0, 180, 0);
                    text.text = "TO EDIT";

                    button.interactable = true;
                } else {
                    GameObject tooltip = faceSelector.transform.Find("Tooltip").gameObject;
                    Button button = tooltip.GetOrAddComponent<Button>();

                    Image image = tooltip.GetComponentInChildren<Image>(true);
                    image.transform.localEulerAngles = new Vector3(0, 0, 0);
                    image.overrideSprite = null;

                    button.interactable = false;
                    button.targetGraphic = null;
                }

                faceSelector.GetComponent<SimulatedSelection>().Select();
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        [HarmonyPrefix]
        public static bool UpdatePrefix(CharacterSelectionInstance __instance) {
            if(__instance.currentPlayer == null) {
                return false;
            }

            GameObject[] faceSelectorArray = __instance.transform.GetChild(0).GetChild(0).GetComponentsInChildren<HoverEvent>(true)
                                .Select(x => x.gameObject)
                                .ToArray();

            if(RequestColorChange.ContainsKey(__instance) && RequestColorChange[__instance]) {
                foreach(GameObject faceSelector in faceSelectorArray) {
                    faceSelector.transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = __instance.currentPlayer.GetTeamColors().color;
                    faceSelector.transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{(GameModeManager.CurrentHandler.AllowTeams ? "TEAM " : "")}{ExtraPlayerSkins.GetTeamColorName(__instance.currentPlayer.colorID()).ToUpper()}";
                }

                if(RequestColorChange.ContainsKey(__instance)) {
                    RequestColorChange[__instance] = false;
                } else {
                    RequestColorChange.Add(__instance, false);
                }
            }

            if(__instance.currentPlayer.GetComponent<PlayerAPI>().enabled) {
                __instance.currentPlayer.data.playerVel.SetFieldValue("simulated", false);
                if(__instance.currentPlayer.data.playerActions == null) {
                    __instance.currentPlayer.data.playerActions = new PlayerActions();
                    __instance.currentPlayer.data.playerActions.Device = InputDevice.Null;
                }

                if(Input.GetKeyDown(KeyCode.R)) {
                    AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(CharacterSelectionInstance __instance) {
            GameObject faceSelector = __instance.transform.GetChild(0).GetChild(0).gameObject;

            for(int j = 0; j < faceSelector.transform.childCount; j++) {
                Button button = faceSelector.transform.GetChild(j).GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    if(__instance.currentPlayer == null) return;
                    if(__instance.currentPlayer.data.GetAdditionalData().IsBot) {
                        UnityEngine.Debug.Log("The bot have be clicked.");
                        button.gameObject.GetComponent<ScaleShake>().AddForce();
                    }
                });
            }
        }
    }
}
