using HarmonyLib;
using System.Collections.Generic;

namespace RoundsWithBots.Pacthes.Storage {
    [HarmonyPatch(typeof(ProjectileHit))]
    public class StoreBullets {
        private readonly static List<ProjectileHit> bullets = new List<ProjectileHit>();
        public static List<ProjectileHit> Bullets {
            get {
                bullets.RemoveAll(b => b == null);
                return bullets;
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void BulletStartPrefix(ProjectileHit __instance) {
            bullets.Add(__instance);
        }
    }
}
