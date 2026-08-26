using HarmonyLib;
using RoundsWithBots.Pacthes.Storage;
using System.Collections.Generic;

namespace RoundsWithBots.Pacthes {
    // Since Landfall uses the "Object.FindObjectsOfType" method, which can become very expensive
    // we just going replace it with our own really optimized implementation.
    //
    // This implementation keep tracks active projectiles instead.
    [HarmonyPatch(typeof(PlayerAPI), nameof(PlayerAPI.GetAllBullets))]
    public class PlayerAPIGetAllBulletsPatch {
        private static readonly AccessTools.FieldRef<ProjectileHit, MoveTransform> MoveRef =
            AccessTools.FieldRefAccess<ProjectileHit, MoveTransform>("move");

        static bool Prefix(ref List<BulletWrapper> __result) {
            var bullets = StoreBullets.Bullets;
            var result = new List<BulletWrapper>(bullets.Count);

            foreach (ProjectileHit projectileHit in bullets) {
                MoveTransform movement = MoveRef(projectileHit);

                result.Add(new BulletWrapper {
                    projectileHit = projectileHit,
                    projectileMovement = movement,
                    damage = projectileHit.damage,
                    velocity = movement.velocity
                });
            }

            __result = result;
            return false;
        }
    }
}
