using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes {
    public class EdgeNode : Node {
        public enum Direction {
            Left,
            Right,
        }

        public Direction EdgeDirection;
        public bool IsCeilingEdge;

        public EdgeNode(int LineIndex, Vector2 position, Direction edgeDirection, bool isCeilingEdge) : base(-1, LineIndex, true, position) {
            EdgeDirection = edgeDirection;
            IsCeilingEdge = isCeilingEdge;
        }
    }
}
