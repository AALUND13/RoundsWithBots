using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using RoundsWithBots.Utils.CardPickerAIs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;

namespace RoundsWithBots.Utils {
    public partial class BotAIManager {
        public float initialDelay = 0.25f;
        public float cycleDelay = 0.3f;
        public float goToDelay = 0.2f;
        public float pickDelay = 0.5f;

        public List<GameObject> GetSpawnCards() {
            Logger.Log("Getting spawn cards");
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        }

        public IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards) {
            Logger.Log("Cycling through cards");

            CardInfo lastCardInfo = null;
            int index = 0;

            foreach(var cardObject in spawnedCards) {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                Logger.Log($"Cycling through '${cardInfo.cardName}' card");
                if(lastCardInfo != null) {
                    lastCardInfo.RPCA_ChangeSelected(false);
                }
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, index);

                lastCardInfo = cardInfo;
                index++;
                yield return new WaitForSeconds(delay);
            }
            Logger.Log("Successfully gone through all cards");
            yield break;
        }
        public IEnumerator GoToCards(GameObject selectedCards, List<GameObject> spawnedCards, float delay) {
            Logger.Log($"Going to '${selectedCards}' card");

            // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
            int selectedCardIndex = spawnedCards.IndexOf(selectedCards);
            int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString());

            while(handIndex != selectedCardIndex) {
                CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(false);
                Logger.Log($"Currently on '${cardInfo}' card");
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
            Logger.Log($"Successfully got to '${selectedCards}' card");
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
                        Logger.Log("AI picking card");
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
                Logger.Log("Bot card picker AI is null, Skipping card picking");
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

            yield return CycleThroughCards(cycleDelay, spawnCards);
            yield return GoToCards(spawnCards[position], spawnCards, goToDelay);
            yield return new WaitForSeconds(pickDelay);

            PickCard(spawnCards);

            yield break;
        }

        [UnboundRPC]
        public static void RPCA_PickCardsAtPosition(int position) {
            instance.StartCoroutine(instance.PickCardsAtPosition(position));
        }
    }
}

