using HarmonyLib;
using Photon.Pun;
using RoundsWithBots.CardPickerAIs;
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
            LoggingUtils.Log("Getting spawn cards");
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        }

        public IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards) {
            LoggingUtils.Log("Cycling through cards");

            CardInfo lastCardInfo = null;
            int index = 0;

            foreach(var cardObject in spawnedCards) {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                LoggingUtils.Log($"Cycling through '${cardInfo.cardName}' card");
                if(lastCardInfo != null) {
                    lastCardInfo.RPCA_ChangeSelected(false);
                }
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, index);

                lastCardInfo = cardInfo;
                index++;
                yield return new WaitForSeconds(delay);
            }
            LoggingUtils.Log("Successfully gone through all cards");
            yield break;
        }
        public IEnumerator GoToCards(GameObject selectedCards, List<GameObject> spawnedCards, float delay) {
            LoggingUtils.Log($"Going to '${selectedCards}' card");

            // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
            int selectedCardIndex = spawnedCards.IndexOf(selectedCards);
            int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString());

            while(handIndex != selectedCardIndex) {
                CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(false);
                LoggingUtils.Log($"Currently on '${cardInfo}' card");
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
            LoggingUtils.Log($"Successfully got to '${selectedCards}' card");
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
                    if(BotPickerAIs.ContainsKey(CardChoice.instance.pickrID)) {
                        LoggingUtils.Log("AI picking card");
                        List<GameObject> spawnCards = GetSpawnCards();
                        spawnCards[0].GetComponent<CardInfo>().GetComponent<PhotonView>().RPC("RPCA_ChangeSelected", RpcTarget.All, true);

                        ICardPickerAI botCardPickerAI = BotPickerAIs[CardChoice.instance.pickrID];
                        StartCardsPicking(spawnCards, botCardPickerAI);

                        break;
                    }
                }
                yield break;
            }
        }
        public void StartCardsPicking(List<GameObject> spawnCards, ICardPickerAI botCardPickerAI) {
            if(botCardPickerAI == null) {
                LoggingUtils.Log("Bot card picker AI is null, Skipping card picking");
                return;
            }

            List<CardInfo> cards = spawnCards.Select(card => card.GetComponent<CardInfo>()).ToList();
            List<CardInfo> pickCards = botCardPickerAI.PickCard(cards);

            CardInfo pickCard = pickCards.ElementAt(Random.Range(0, pickCards.Count));
            int pickCardIndex = cards.IndexOf(pickCard);

            NetworkingManager.RPC(typeof(BotAIManager), nameof(RPCA_PickCardsAtPosition), pickCardIndex);
        }

        private IEnumerator PickCardsAtPosition(int position) {
            List<GameObject> spawnCards = GetSpawnCards();

            yield return CycleThroughCards(RWBMenu.CycleDelay.Value, spawnCards);
            yield return new WaitForSeconds(RWBMenu.PreCycleDelay.Value);

            yield return GoToCards(spawnCards[position], spawnCards, RWBMenu.GoToCardDelay.Value);
            yield return new WaitForSeconds(RWBMenu.PickDelay.Value);

            PickCard(spawnCards);
            
            yield break;
        }

        [UnboundRPC]
        public static void RPCA_PickCardsAtPosition(int position) {
            Instance.StartCoroutine(Instance.PickCardsAtPosition(position));
        }
    }
}

