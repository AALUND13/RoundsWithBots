//using HarmonyLib;

//namespace RoundWithBot.Pacthes.RWF {

//    [HarmonyPatch(typeof(PlayerSpotlight))]
//    internal class RWFAddSpotToPlayerPatch {
//        [HarmonyPatch("AddSpotToPlayer")]

//        private static bool Prefix(Player player) {

//            return !player.GetComponent<PlayerAPI>().enabled;
//        }
//    }
//}