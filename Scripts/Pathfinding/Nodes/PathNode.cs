using System.Collections.Generic;

namespace RoundsWithBots.Pathfinding.Nodes {
    public class PathNode {
        public Node Node;
        public List<PathNode> ConnetedNodes = new List<PathNode>();

        public PathNode(Node node) {
            Node = node;
        }

        public void ConnectNode(PathNode node) {
            ConnetedNodes.Add(node);
            node.ConnetedNodes.Add(this);
        }

        public PathNode[] GetVerticalNodes() {
            List<PathNode> nodes = new List<PathNode>() { this };

            foreach (PathNode connected in ConnetedNodes) {
                if (!(connected.Node is EdgeNode) || connected.Node.LineIndex != Node.LineIndex)
                    continue;

                nodes.Add(connected);
            }

            return nodes.ToArray();
        }
    }
}
