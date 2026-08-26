using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes {
    public abstract class Node {
        public int ObjectId;
        public int LineIndex;
        public bool Vaild;

        public Vector2 Position;

        public Node(int id, int lineIndex, bool vaild, Vector2 position) {
            ObjectId = id;
            LineIndex = lineIndex;
            Position = position;
            Vaild = vaild;
        }
    }
}
