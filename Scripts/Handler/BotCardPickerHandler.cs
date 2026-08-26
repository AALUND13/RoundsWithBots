using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using RoundsWithBots.CardPickerAIs;
using RoundsWithBots.CardPickerAIs.Weighted;
using RoundsWithBots.Menu;
using RoundsWithBots.Mods.Compatibility;
using RoundsWithBots.Utils.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnityEngine;

namespace RoundsWithBots.Handlers {
    public class BotCardPickerHandler : MonoBehaviour {
        private class PickerInfo {
            public float CycleDelay;
            public float PreCycleDelay;
            public float GoToCardDelay;
            public float PickDelay;

            public PickerInfo(float cycleDelay, float preCycleDelay, float goToCardDelay, float pickDelay) {
                CycleDelay = cycleDelay;
                PreCycleDelay = preCycleDelay;
                GoToCardDelay = goToCardDelay;
                PickDelay = pickDelay;
            }

            public PickerInfo() {
                CycleDelay = RWBMenu.CycleDelay.Value;
                PreCycleDelay = RWBMenu.PreCycleDelay.Value;
                GoToCardDelay = RWBMenu.GoToCardDelay.Value;
                PickDelay = RWBMenu.PickDelay.Value;
            }
        }

        private class CardPickerAI {
            public ICardPickerAI cardPickerAI;
            public PickerInfo pickerInfo;

            public CardPickerAI(ICardPickerAI cardPickerAI, PickerInfo pickerInfo) {
                this.cardPickerAI = cardPickerAI;
                this.pickerInfo = pickerInfo;
            }

            public CardPickerAI() {
                cardPickerAI = new WeightedCardsPicker();
                pickerInfo = new PickerInfo();
            }
        }


        public static BotCardPickerHandler Instance { get; private set; }


        public List<GameObject> SpawnedCard =>
            (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        public Transform[] Children =>
            (Transform[])CardChoice.instance.GetFieldValue("children");

        public int CurrentSelectedCard {
            get => (int)AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance);
            set => AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, value);
        }

        private bool pickerAIPicking = false;
        private CardPickerAI PickerAI;


        private void Awake() {
            if (Instance != null) {
                Destroy(Instance);
                return;
            } else {
                Instance = this;
            }

            if (RoundsWithBots.Plugins.Any(plugin => plugin.Info.Metadata.GUID == "Systems.R00t.PickPhaseImprovements")) {
                PickPhaseImprovementsInterrface.RegisterHandFinalizationAction((_) => StartCoroutine(AiPickCard()));
            } else {
                GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, PlayerPickStart);
            }
            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);
        }


        private IEnumerator OnGameStart(IGameModeHandler gameModeHandler) {
            pickerAIPicking = false;
            PickerAI = new CardPickerAI();
            yield break;
        }

        private IEnumerator PlayerPickStart(IGameModeHandler gameModeHandler) {
            StartCoroutine(Instance.AiPickCard());
            yield break;
        }


        private IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards) {
            CardInfo lastCardInfo = null;

            for (int i = 0; i < spawnedCards.Count; i++) {
                CardInfo cardInfo = spawnedCards[i].GetComponent<CardInfo>();

                if (lastCardInfo != null) {
                    lastCardInfo.RPCA_ChangeSelected(false);
                }
                cardInfo.RPCA_ChangeSelected(true);
                lastCardInfo = cardInfo;
                CurrentSelectedCard = i;

                yield return new WaitForSeconds(delay);
            }

            yield break;
        }

        private IEnumerator GoToCards(GameObject selectedCards, List<GameObject> spawnedCards, float delay) {
            int selectedCardIndex = spawnedCards.IndexOf(selectedCards);

            while (CurrentSelectedCard != selectedCardIndex) {
                CardInfo cardInfo = spawnedCards[CurrentSelectedCard].GetComponent<CardInfo>();

                cardInfo.RPCA_ChangeSelected(false);
                if (CurrentSelectedCard > selectedCardIndex) {
                    CurrentSelectedCard--;
                } else if (CurrentSelectedCard < selectedCardIndex) {
                    CurrentSelectedCard++;
                }
                cardInfo = spawnedCards[CurrentSelectedCard].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(true);

                yield return new WaitForSeconds(delay);
            }
            yield break;
        }

        private void PickCard() {
            pickerAIPicking = false;
            CardChoice.instance.Pick(SpawnedCard[CurrentSelectedCard], true);
        }

        private IEnumerator AiPickCard() {
            if ((PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) && !pickerAIPicking) {
                if (BotsManager.Instance.BotList.Any(b => b.playerID == CardChoice.instance.pickrID)) {
                    pickerAIPicking = true;
                }

                yield return new WaitUntil(() => {
                    return CardChoice.instance.IsPicking && SpawnedCard.Count == Children.Length && !SpawnedCard.Any(card => { return card == null; });
                }); //wait untill all the cards are generated

                for (int i = 0; i < PlayerManager.instance.players.Count; i++) {
                    Player player = PlayerManager.instance.players[i];
                    if (pickerAIPicking == true) {
                        SpawnedCard[0].GetComponent<CardInfo>().GetComponent<PhotonView>().RPC("RPCA_ChangeSelected", RpcTarget.All, true);

                        StartCardsPicking(SpawnedCard, PickerAI.cardPickerAI, PickerAI.pickerInfo);
                        break;
                    }
                }

                yield break;
            }
        }

        private void StartCardsPicking(List<GameObject> spawnCards, ICardPickerAI botCardPickerAI, PickerInfo pickerInfo) {
            if (botCardPickerAI == null) {
                LoggerUtils.Log(LogLevel.Warning, "Bot card picker AI is null, Skipping card picking");
                return;
            }

            List<CardInfo> cards = spawnCards.Select(card => card.GetComponent<CardInfo>()).ToList();
            List<CardInfo> pickCards = botCardPickerAI.PickCard(cards);

            CardInfo pickCard = pickCards.ElementAt(Random.Range(0, pickCards.Count));
            int pickCardIndex = cards.IndexOf(pickCard);

            NetworkingManager.RPC(typeof(BotCardPickerHandler), nameof(RPCA_PickCardsAtPosition), pickCardIndex, pickerInfo.CycleDelay, pickerInfo.PreCycleDelay, pickerInfo.GoToCardDelay, pickerInfo.PickDelay);
        }

        private IEnumerator PickCardsAtPosition(int position, float cycleDelay, float preCycleDelay, float goToCardDelay, float pickDelay) {
            yield return CycleThroughCards(cycleDelay, SpawnedCard);
            yield return new WaitForSeconds(preCycleDelay);

            yield return GoToCards(SpawnedCard[position], SpawnedCard, goToCardDelay);
            yield return new WaitForSeconds(pickDelay);

            PickCard();

            yield break;
        }

        [UnboundRPC]
        private static void RPCA_PickCardsAtPosition(int position, float cycleDelay, float preCycleDelay, float goToCardDelay, float pickDelay) {
            Instance.StartCoroutine(Instance.PickCardsAtPosition(position, cycleDelay, preCycleDelay, goToCardDelay, pickDelay));
        }
    }
}

