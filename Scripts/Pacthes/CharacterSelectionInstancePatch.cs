
using HarmonyLib;
using RoundsWithBots.Extensions;
using RoundsWithBots.Menu;
using RoundsWithBots.UI;
using System.Collections.Generic;
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
        public static Dictionary<CharacterSelectionInstance, BotMenuUIHandler> BotMenuMap = new Dictionary<CharacterSelectionInstance, BotMenuUIHandler>();
        public static GameObject BotMenusCanvas;

        [HarmonyPatch("StartPicking")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        [HarmonyPostfix]
        public static void StartPickingPostfix(CharacterSelectionInstance __instance, HoverEvent[] ___buttons) {
            if (__instance.currentPlayer.data.GetAdditionalData().IsBot) {
                if (RWBMenu.RandomizationFace.Value) {
                    __instance.currentlySelectedFace = Random.Range(0, 7);
                } else {
                    __instance.currentlySelectedFace = RWBMenu.SelectedFace.Value;
                }

                if (!BotMenuMap.ContainsKey(__instance)) {
                    BotMenuUIHandler botMenu = BotMenuUIHandler.CreateInstance(__instance);
                    botMenu.transform.SetParent(__instance.transform, false);

                    RectTransform rect = botMenu.GetComponent<RectTransform>();
                    botMenu.ExecuteAfterFrames(1, () => {
                        rect.anchoredPosition = new Vector2(0, 95);
                    });

                    BotMenuMap.Add(__instance, botMenu);
                } else {
                    BotMenuUIHandler botMenu = BotMenuMap[__instance];
                    botMenu.HideMenu();
                    botMenu.Refresh();
                }
            } else if (BotMenuMap.ContainsKey(__instance)) {
                GameObject.Destroy(BotMenuMap[__instance]);
                BotMenuMap.Remove(__instance);
            }

            __instance.ExecuteAfterFrames(1, () => {
                __instance.transform.GetChild(0).GetChild(1).gameObject.SetActive(false); // This line hides the arrow that the player can uses the buttons
                SetupButton(__instance, ___buttons);
            });
        }

        private static void SetupButton(CharacterSelectionInstance __instance, HoverEvent[] ___buttons) {
            foreach (HoverEvent faceSelector in ___buttons) {
                if (__instance.currentPlayer.data.GetAdditionalData().IsBot) {
                    GameObject tooltip = faceSelector.transform.Find("Tooltip").gameObject;
                    Button button = tooltip.GetOrAddComponent<Button>();

                    Image image = tooltip.GetComponentInChildren<Image>(true);
                    TextMeshProUGUI text = tooltip.GetComponentInChildren<TextMeshProUGUI>();
                    ControllerImageToggler controllerImageToggler = image.GetComponent<ControllerImageToggler>();

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        Vector2 cursorPosition = Input.mousePosition;

                        if (BotMenuMap.TryGetValue(__instance, out BotMenuUIHandler botMenu)) {
                            botMenu.OpenMenu();
                        }
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
        public static bool UpdatePrefix(CharacterSelectionInstance __instance, HoverEvent[] ___buttons) {
            if (__instance.currentPlayer == null) {
                return false;
            }

            if (RequestColorChange.ContainsKey(__instance) && RequestColorChange[__instance]) {
                foreach (HoverEvent faceSelector in ___buttons) {
                    faceSelector.transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = __instance.currentPlayer.GetTeamColors().color;
                    faceSelector.transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{(GameModeManager.CurrentHandler.AllowTeams ? "TEAM " : "")}{ExtraPlayerSkins.GetTeamColorName(__instance.currentPlayer.colorID()).ToUpper()}";
                }

                if (RequestColorChange.ContainsKey(__instance)) {
                    RequestColorChange[__instance] = false;
                } else {
                    RequestColorChange.Add(__instance, false);
                }
            }

            if (__instance.currentPlayer.data.GetAdditionalData().IsBot) {
                __instance.currentPlayer.data.playerVel.SetFieldValue("simulated", false);

                if (Input.GetKeyDown(KeyCode.R)) {
                    AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                    return false;
                }
            }

            return true;
        }
    }
}
