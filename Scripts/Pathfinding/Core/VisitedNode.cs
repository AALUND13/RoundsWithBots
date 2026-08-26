using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Utils;
using UnityEngine;

namespace RoundsWithBots.Pathfinding.Core {
    public class VisitedNode : IHeapItem<VisitedNode> {
        public PathNode PathNode;
        public VisitedNode Parent;

        public int HeapIndex { get; set; }

        public float G = float.PositiveInfinity;
        public float H = float.PositiveInfinity;
        public float F => G + H;

        public float JumpDist;

        public VisitedNode(PathNode node, VisitedNode parent) {
            PathNode = node;
            Parent = parent;
        }

        public VisitedNode(PathNode node) : this(node, null) { }

        public float GetPenalty(PathfindingOptions penaltyOption) {
            if (Parent == null) return 0;

            float cost = 0;

            cost += Mathf.Pow(JumpDist, penaltyOption.JumpPenaltyPow);
            if (PathNode.Node is AirNode) {
                cost += GetDistance(Parent.PathNode) * penaltyOption.AirNodePenaltyMult;
            }

            return cost;
        }

        public float GetDistance(PathNode b) {
            return Vector2.Distance(PathNode.Node.Position, b.Node.Position);
        }

        public int CompareTo(VisitedNode other) {
            int fCompare = F.CompareTo(other.F);

            if (fCompare != 0)
                return fCompare;

            return H.CompareTo(other.H);
        }
    }
}
