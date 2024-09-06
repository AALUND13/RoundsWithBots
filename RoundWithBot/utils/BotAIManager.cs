using HarmonyLib;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unbound.Core;
using UnityEngine;

namespace RoundWithBot.utils {
    public class BotAIManager {
        public static List<int> botsId = new List<int>();
        public static List<CardInfo> excludeCards = new List<CardInfo>();

        public static void AddExcludeCard(CardInfo excludeCard) {
            if(excludeCard == null) {
                throw new System.ArgumentNullException("excludeCard", "The exclude card can't be null");
            }

            excludeCard.categories = excludeCard.categories.AddItem(RoundWithBots.NoBot).ToArray();
            excludeCards.Add(excludeCard);

            Logger.Log("'" + excludeCard.CardName + "' Have be added to the exclude cards");
        }
        public static void AddExcludeCard(string excludeCardName) {
            CardInfo card = Unbound.Cards.Utils.CardManager.GetCardInfoWithName(excludeCardName);
            AddExcludeCard(card);
        }

        public static bool IsAExcludeCard(CardInfo card) {
            return (CardChoice.instance.pickrID != -1 && botsId.Contains(CardChoice.instance.pickrID)) &&
                   (excludeCards.Any(excludeCard => excludeCard.CardName == card.CardName) ||
                   card.blacklistedCategories.Contains(RoundWithBots.NoBot));
        }

        public static void SetBotsId() {
            Logger.Log("Getting bots player.");

            botsId.Clear();
            botsId = PlayerManager.instance.players
                     .Where(player => player.GetComponent<PlayerAPI>().enabled)
                     .Select(player => player.playerID)
                     .ToList();

            botsId.ForEach(id => Logger.Log($"Bot '{id}' has been added to the list of bots id."));
            Logger.Log("Successfully get list of bots player.");
        }

        public static List<GameObject> GetRarestCards(List<GameObject> spawnCards) {
            Logger.Log("getting rarest cards...");

            List<GameObject> rarestCards = spawnCards
                 .GroupBy(card => card.GetComponent<CardInfo>().rarity)
                 .OrderBy(group => group.Key)
                 .Reverse()
                 .FirstOrDefault()?.ToList() ?? new List<GameObject>();

            return rarestCards;
        }

        public static List<GameObject> GetSpawnCards() {
            Logger.Log("Getting spawn cards");
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        }

        public static IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards) {
            Logger.Log("Cycling through cards");

            CardInfo lastCardInfo = null;
            int index = 0;

            foreach(var cardObject in spawnedCards) {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                Logger.Log("Cycling through '" + cardInfo.CardName + "' card");
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

        public static IEnumerator GoToCards(List<GameObject> rarestCards, List<GameObject> spawnedCards, float delay) {
            int randomIndex = Random.Range(0, rarestCards.Count - 1);
            GameObject cardToPick = rarestCards[randomIndex];
            Logger.Log("Going to '" + cardToPick + "' card");

            // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
            int selectedCardIndex = spawnedCards.IndexOf(cardToPick);
            int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString());

            while(handIndex != selectedCardIndex) {
                CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(false);
                Logger.Log("Currently on '" + cardInfo + "' card");
                if(handIndex > selectedCardIndex) {
                    handIndex--;
                } else if(handIndex < selectedCardIndex) {
                    handIndex++;
                }
                cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, handIndex);

                // Wait for some time before the next iteration
                yield return new WaitForSeconds(delay); // Adjust the time as needed
            }
            Logger.Log("Successfully got to '" + cardToPick + "' card");
            yield break;
        }

        public static IEnumerator PickCard(List<GameObject> spawnCards) {
            CardChoice.instance.Pick(spawnCards[(int)CardChoice.instance.GetFieldValue("currentlySelectedCard")], true);
            yield break;
        }
        
        public static IEnumerator PickBestCard(List<GameObject> spawnCards, float initialDelay, float cycleDelay, float goToDelay, float pickDelay) {
        yield return new WaitForSeconds(initialDelay);
            yield return CycleThroughCards(cycleDelay, spawnCards);
            List<GameObject> rarestCards = GetRarestCards(spawnCards);
            yield return GoToCards(rarestCards, spawnCards, goToDelay);
            yield return new WaitForSeconds(pickDelay);
            yield return PickCard(spawnCards);
        }

        public static IEnumerator AiPickCard() {
            yield return new WaitUntil(() => {
                return CardChoice.instance.IsPicking &&
               ((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Count == ((Transform[])CardChoice.instance.GetFieldValue("children")).Count() &&
               !((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Any(card => { return card == null; });
            }); //wait untill all the cards are generated
            for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                Player player = PlayerManager.instance.players[i];
                if(player.GetComponent<PlayerAPI>().enabled && botsId.Contains(CardChoice.instance.pickrID)) {

                    Logger.Log("AI picking card");
                    List<GameObject> spawnCards = GetSpawnCards();
                    spawnCards[0].GetComponent<CardInfo>().RPCA_ChangeSelected(true);
                    yield return PickBestCard(spawnCards, 0.25f, 0.3f, 0.2f, 0.5f);
                    break;
                }
            }
            yield break;
        }
    }
}

