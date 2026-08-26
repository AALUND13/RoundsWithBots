using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes {
    public class AirNode : Node {
        public bool IsEdgeAirNode = false;
        public AirNode(int LineIndex, Vector2 position, bool isEdgeAirNode) : base(-1, LineIndex, true, position) {
            IsEdgeAirNode = isEdgeAirNode;
        }
    }
}
