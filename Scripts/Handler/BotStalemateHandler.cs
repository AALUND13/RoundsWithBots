using ModdingUtils.GameModes;
using ModdingUtils.Utils;
using Photon.Pun;
using RoundsWithBots.Extensions;
using RoundsWithBots.Menu;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;

namespace RoundsWithBots.Handlers {
    public class BotStalemateHandler : MonoBehaviour, IPointStartHookHandler {
        public static BotStalemateHandler Instance { get; private set; }

        public float StalemateTimer => RWBMenu.StalemateTimer.Value;
        public float StalemateDamageCooldown => RWBMenu.StalemateDamageCooldown.Value;
        public float StalemateDamageDuration => RWBMenu.StalemateDamageDuration.Value;

        public static bool IsAllPlayersBots =>
            PlayerManager.instance.players.All(player => player.data.GetAdditionalData().IsBot);

        private float BattleStartedAt = 0;
        private float LastDamageBotAt = 0;
        private void Awake() {
            if (Instance != null) {
                Destroy(Instance);
                return;
            } else {
                Instance = this;
            }

            InterfaceGameModeHooksManager.instance.RegisterHooks(this);
        }

        private void OnDestroy() {
            InterfaceGameModeHooksManager.instance.RemoveHooks(this);
        }

        private void Update() {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) {
                if (Time.time < BattleStartedAt + StalemateTimer)
                    return;
                if (Time.time < LastDamageBotAt + StalemateDamageCooldown)
                    return;
                if (!IsAllPlayersBots)
                    return;

                Player[] aliveBots = PlayerManager.instance.players
                    .Where(player => PlayerStatus.PlayerAliveAndSimulated(player))
                    .ToArray();

                if (aliveBots.Length > 0) {
                    Player targetBot = aliveBots[Random.Range(0, aliveBots.Length)];
                    NetworkingManager.RPC(typeof(BotStalemateHandler), nameof(RPCA_SendTakeDamageOverTime), targetBot.playerID, targetBot.data.maxHealth, StalemateDamageDuration);

                    LastDamageBotAt = Time.time;
                }
            }
        }


        public void OnPointStart() {
            BattleStartedAt = Time.time;
        }


        [UnboundRPC]
        public static void RPCA_SendTakeDamageOverTime(int playerID, float damage, float duration) {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            player.data.healthHandler.TakeDamageOverTime(damage * Vector2.down, player.gameObject.transform.position, duration, 0.5f, Color.white);
        }
    }
}
