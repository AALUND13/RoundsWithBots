namespace RoundsWithBots.Pathfinding.Core {
    public struct PathfindingOptions {
        // Penalty Options
        public float JumpPenaltyPow;
        public float JumpMeters;
        public float AirNodePenaltyMult;

        // Other Options
        public float MinSpaceRequire;

        public PathfindingOptions(float jumpPenaltyPow, float airNodePenaltyMult, float jumpMeters, float minSpaceRequire) {
            JumpPenaltyPow = jumpPenaltyPow;
            AirNodePenaltyMult = airNodePenaltyMult;
            JumpMeters = jumpMeters;
            MinSpaceRequire = minSpaceRequire;
        }
    }
}
