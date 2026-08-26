using HarmonyLib;
using RoundsWithBots.Pacthes.Storage;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RoundsWithBots.AI {
    public class AIBlockModule : AIModuleBase {
        [Header("Blocking Option")]
        public float MinDistanceFromBoraderToBlock = 2.0f;
        public float MinDistanceFromBulletsToBlock = 0.7f;


        private void Update() {
            if ((IsThereIncommingBullets() || IsPlayerNearBorder()) && !InputLocked()) {
                playerAPI.Block();
            }
        }


        private bool InputLocked() {
            return GameManager.lockInput || playerAPI.data.input.stunnedInput || playerAPI.data.input.silencedInput || !playerAPI.data.isPlaying;
        }

        private bool IsThereIncommingBullets() {
            Vector2 playerPos = playerAPI.PlayerPosition();
            float distanceForBlock = (transform.parent.localScale.y / 2f) + MinDistanceFromBulletsToBlock;
            float radiusSqr = distanceForBlock * distanceForBlock;

            List<BulletWrapper> allBullets = playerAPI.GetAllBullets();
            foreach (var bullet in allBullets) {
                Vector2 bulletPos = bullet.projectileHit.transform.position;
                Vector2 toPlayer = playerPos - bulletPos;

                if (bullet.projectileHit.ownPlayer == playerAPI.player)
                    continue;
                if (toPlayer.sqrMagnitude <= radiusSqr)
                    return true;
            }

            return false;
        }

        private bool IsPlayerNearBorder() {
            OutOfBoundsHandler outOfBoundsHandlerInstance = StoreOutOfBoundsHandler.outOfBoundsHandlers[playerAPI.player];

            MethodInfo getPointMethod = AccessTools.Method(typeof(OutOfBoundsHandler), "GetPoint");
            float left = ((Vector3)getPointMethod.Invoke(outOfBoundsHandlerInstance, new object[] { new Vector3(0f, 0f, 0f) })).x;
            float right = ((Vector3)getPointMethod.Invoke(outOfBoundsHandlerInstance, new object[] { new Vector3(1f, 0f, 0f) })).x;
            float bottom = ((Vector3)getPointMethod.Invoke(outOfBoundsHandlerInstance, new object[] { new Vector3(0f, 0f, 0f) })).y;
            float top = ((Vector3)getPointMethod.Invoke(outOfBoundsHandlerInstance, new object[] { new Vector3(0f, 1f, 0f) })).y;

            float surfaceDistanceWithOffset = (playerAPI.transform.localScale.y / 2) + MinDistanceFromBoraderToBlock;
            bool isNearBoundaries = playerAPI.PlayerPosition().x <= left + surfaceDistanceWithOffset ||
                   playerAPI.PlayerPosition().x >= right - surfaceDistanceWithOffset ||
                   playerAPI.PlayerPosition().y <= bottom + surfaceDistanceWithOffset ||
                   playerAPI.PlayerPosition().y >= top - surfaceDistanceWithOffset;

            return isNearBoundaries;
        }
    }
}
