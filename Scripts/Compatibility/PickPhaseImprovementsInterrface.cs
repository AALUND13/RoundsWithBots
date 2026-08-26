using PickPhaseImprovements;
using System;

namespace RoundsWithBots.Mods.Compatibility {
    public static class PickPhaseImprovementsInterrface {
        public static void RegisterHandFinalizationAction(Action<CardInfo[]> func)
            => PickManager.RegisterHandFinalizationAction(func);
    }
}
