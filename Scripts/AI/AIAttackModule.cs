using RoundsWithBots.Utils;
using UnboundLib;
using UnityEngine;

namespace RoundsWithBots.AI {
    public class AIAttackModule : AIModuleBase {
        private void Update() {
            AttemptAttack();
        }

        private void AttemptAttack() {
            Player otherPlayer = playerAPI.GetOtherPlayer();
            if (otherPlayer == null) return;

            Vector2 otherPlayerPos = otherPlayer.transform.position;

            Gun gun = playerAPI.data.weaponHandler.gun;
            MoveTransform bulletTransform = gun.projectiles[0].objectToSpawn.GetComponent<MoveTransform>();

            if (ProjectileAimUtils.TryGetAimDirection(
                transform.position,
                otherPlayerPos + (Vector2)otherPlayer.data.playerVel.GetFieldValue("velocity") * 0.1f,
                bulletTransform.localForce.magnitude * playerAPI.data.weaponHandler.gun.projectileSpeed,
                bulletTransform.gravity * playerAPI.data.weaponHandler.gun.gravity, 
                playerAPI.data.weaponHandler.gun.drag, 
                playerAPI.data.weaponHandler.gun.dragMinSpeed,
                playerAPI.data.weaponHandler.gun.projectielSimulatonSpeed,
                2.5f,
                0.02f,
                out Vector2 direction
            )) {
                playerAPI.SetAimDirection(direction);
                if (CanSeeTarget(playerAPI.PlayerPosition(), otherPlayerPos)
                    && !otherPlayer.data.block.IsBlocking()
                    && !otherPlayer.data.dead
                    && !otherPlayer.data.healthHandler.isRespawning
                    && !InputLocked()
                ) {
                    playerAPI.Attack();
                }
            } else {
                playerAPI.SetAimDirection((playerAPI.TowardsOtherPlayer() + (Vector2)otherPlayer.data.playerVel.GetFieldValue("velocity") * 0.1f).normalized);
            }
        } 

        private bool InputLocked() {
            return GameManager.lockInput || playerAPI.data.input.stunnedInput || playerAPI.data.input.silencedInput || !playerAPI.data.isPlaying;
        }
    }
}
