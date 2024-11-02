﻿using ModdingUtils.Utils;
using Photon.Pun;
using RoundsWithBots.Extensions;
using RoundsWithBots.Menu;
using RWF;
using System.Collections;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;

namespace RoundsWithBots {
    internal static class StalemateHandler {
        public static bool IsStalemate {
            get {
                bool isPlayersAlive = PlayerManager.instance.players
                    .Any(player => !player.data.GetAdditionalData().IsBot && PlayerStatus.PlayerAliveAndSimulated(player));

                return !isPlayersAlive;
            }
        }

        public static IEnumerator HandleStalemate() {
            if(PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) {
                yield return new WaitForSeconds(2f);

                while(!IsStalemate) {
                    yield return new WaitForSeconds(0.5f);
                }

                yield return new WaitForSeconds(RWBMenu.StalemateTimer.Value);

                while(IsStalemate) {
                    yield return new WaitForSeconds(RWBMenu.StalemateDamageCooldown.Value);

                    Player[] aliveBots = PlayerManager.instance.players
                        .Where(player => PlayerStatus.PlayerAliveAndSimulated(player))
                        .ToArray();

                    // If there are no bots alive, break out of the loop
                    if(aliveBots.Length == 0) {
                        yield break;
                    }

                    Player player = aliveBots[Random.Range(0, aliveBots.Length)];
                    NetworkingManager.RPC(typeof(StalemateHandler), nameof(RPCA_SendTakeDamageOverTime), player.data.view.ControllerActorNr, player.playerID, player.data.maxHealth, RWBMenu.StalemateDamageDuration.Value);
                }
            }
            yield break;
        }

        [UnboundRPC]
        public static void RPCA_SendTakeDamageOverTime(int actorID, int playerID, float damage, float duration) {
            Player player = FindPlayer.GetPlayerWithActorAndPlayerIDs(actorID, playerID);
            player.data.healthHandler.TakeDamageOverTime(damage * Vector2.down, player.gameObject.transform.position, duration, 0.5f, Color.white);
        }
    }
}
