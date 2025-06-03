using HarmonyLib;
using Photon.Pun;
using RoundsWithBots.CardPickerAIs;
using RoundsWithBots.CardPickerAIs.Weighted.WeightedCardProcessesor;
using RoundsWithBots.CardPickerAIs.Weighted;
using RoundsWithBots.Menu;
using RoundsWithBots.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;

namespace RoundsWithBots {

    public partial class BotAIManager {
        public List<GameObject> GetSpawnCards() {
            LoggerUtils.Log("Getting spawn cards");
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        }

        public IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards) {
            LoggerUtils.Log("Cycling through cards");

            CardInfo lastCardInfo = null;
            int index = 0;

            foreach(var cardObject in spawnedCards) {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                LoggerUtils.Log($"Cycling through '${cardInfo.cardName}' card");
                if(lastCardInfo != null) {
                    lastCardInfo.RPCA_ChangeSelected(false);
                }
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, index);

                lastCardInfo = cardInfo;
                index++;
                yield return new WaitForSeconds(delay);
            }
            LoggerUtils.Log("Successfully gone through all cards");
            yield break;
        }
        public IEnumerator GoToCards(GameObject selectedCards, List<GameObject> spawnedCards, float delay) {
            LoggerUtils.Log($"Going to '${selectedCards}' card");

            // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
            int selectedCardIndex = spawnedCards.IndexOf(selectedCards);
            int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString());

            while(handIndex != selectedCardIndex) {
                CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(false);
                LoggerUtils.Log($"Currently on '${cardInfo}' card");
                if(handIndex > selectedCardIndex) {
                    handIndex--;
                } else if(handIndex < selectedCardIndex) {
                    handIndex++;
                }
                cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, handIndex);

                // Wait for some time before the next iteration
                yield return new WaitForSeconds(delay);
            }
            LoggerUtils.Log($"Successfully got to '${selectedCards}' card");
            yield break;
        }

        public void PickCard(List<GameObject> spawnCards) {
            CardChoice.instance.Pick(spawnCards[(int)CardChoice.instance.GetFieldValue("currentlySelectedCard")], true);
        }

        public IEnumerator AiPickCard() {
            if(PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) {
                yield return new WaitUntil(() => {
                    return CardChoice.instance.IsPicking &&
                   ((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Count == ((Transform[])CardChoice.instance.GetFieldValue("children")).Count() &&
                   !((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Any(card => { return card == null; });
                }); //wait untill all the cards are generated

                for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                    Player player = PlayerManager.instance.players[i];
                    if(PickerAIs.ContainsKey(CardChoice.instance.pickrID)) {
                        LoggerUtils.Log("AI picking card");
                        List<GameObject> spawnCards = GetSpawnCards();
                        spawnCards[0].GetComponent<CardInfo>().GetComponent<PhotonView>().RPC("RPCA_ChangeSelected", RpcTarget.All, true);

                        ICardPickerAI botCardPickerAI = PickerAIs[CardChoice.instance.pickrID].cardPickerAI;
                        StartCardsPicking(spawnCards, botCardPickerAI, PickerAIs[CardChoice.instance.pickrID].pickerInfo);

                        break;
                    }
                }
                yield break;
            }
        }

        public void StartCardsPicking(List<GameObject> spawnCards, ICardPickerAI botCardPickerAI, PickerInfo pickerInfo) {
            if(botCardPickerAI == null) {
                LoggerUtils.Log("Bot card picker AI is null, Skipping card picking");
                return;
            }

            List<CardInfo> cards = spawnCards.Select(card => card.GetComponent<CardInfo>()).ToList();
            List<CardInfo> pickCards = botCardPickerAI.PickCard(cards);

            CardInfo pickCard = pickCards.ElementAt(Random.Range(0, pickCards.Count));
            int pickCardIndex = cards.IndexOf(pickCard);

            NetworkingManager.RPC(typeof(BotAIManager), nameof(RPCA_PickCardsAtPosition), pickCardIndex, pickerInfo.CycleDelay, pickerInfo.PreCycleDelay, pickerInfo.GoToCardDelay, pickerInfo.PickDelay);
        }

        private IEnumerator PickCardsAtPosition(int position, float cycleDelay, float preCycleDelay, float goToCardDelay, float pickDelay) {
            List<GameObject> spawnCards = GetSpawnCards();

            yield return CycleThroughCards(cycleDelay, spawnCards);
            yield return new WaitForSeconds(preCycleDelay);

            yield return GoToCards(spawnCards[position], spawnCards, goToCardDelay);
            yield return new WaitForSeconds(pickDelay);

            PickCard(spawnCards);

            yield break;
        }

        [UnboundRPC]
        private static void RPCA_PickCardsAtPosition(int position, float cycleDelay, float preCycleDelay, float goToCardDelay, float pickDelay) {
            Instance.StartCoroutine(Instance.PickCardsAtPosition(position, cycleDelay, preCycleDelay, goToCardDelay, pickDelay));
        }
    }

    public class PickerInfo {
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

    public class CardPickerAI {
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
}

