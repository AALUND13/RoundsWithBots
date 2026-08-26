using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes {
    public class CeilingNode : Node {
        public CeilingNode(int id, int LineIndex, bool vaild, Vector2 position) : base(id, LineIndex, vaild, position) { }
    }
}
