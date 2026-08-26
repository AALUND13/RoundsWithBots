using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes {
    public class GroundNode : Node {
        public float spaceAbove;
        public GroundNode(int id, int LineIndex, bool vaild, Vector2 position, float spaceAbove) : base(id, LineIndex, vaild, position) {
            this.spaceAbove = spaceAbove;
        }
    }
}
