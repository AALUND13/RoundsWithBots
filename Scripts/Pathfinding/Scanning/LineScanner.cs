using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Pathfinding.Nodes.Lines;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoundsWithBots.Pathfinding.Scanning {
    public sealed class LineScanner {
        const float MIN_DISTANCE_BETWEEN_NODES = 1f;
        const float AIR_NODES_RANGE = 15;

        public LineScanner PreviousLineScanner;
        public LineScanner NextLineScanner;

        public NodeLine NodeLine;
        public PathLine PathLine;

        public readonly float x;
        public readonly int LineIndex;
        private readonly float metherPerLine;

        private Vector2 verticalLine;
        private Vector2 bound;

        private int mask;

        public LineScanner(float x, int LineIndex, float metherPerLine, Vector2 verticalLine, Vector2 bound, int mask) {
            this.x = x;
            this.LineIndex = LineIndex;
            this.metherPerLine = metherPerLine;
            this.verticalLine = verticalLine;
            this.bound = bound;
            this.mask = mask;
        }


        public void InitializeNeighborScanners(LineScanner previousLineScanner, LineScanner nextLineScanner) {
            if (previousLineScanner != null)
                this.PreviousLineScanner = previousLineScanner;
            if (nextLineScanner != null)
                this.NextLineScanner = nextLineScanner;
        }

        public void BuildNodeLine() {
            List<Node> Nodes = new List<Node>();
            List<Node> vaildNodes = new List<Node>();

            Vector2 start = new Vector2(x, verticalLine.x);
            Vector2 end = new Vector2(x, verticalLine.y);

            // We linecast from top to bottom and bottom to top to include the both top and bottom edges of the colliders in the scan.
            // This is necessary because this pathfinding system is designed to work with colliders both above and below of the colliders that are being scanned.
            RaycastHit2D[] hitsFromTopToBottom = Physics2D.LinecastAll(start, end, mask);
            RaycastHit2D[] hitsFromBottomToTop = Physics2D.LinecastAll(end, start, mask);
            RaycastHit2D[] hits = hitsFromTopToBottom.Concat(hitsFromBottomToTop).OrderByDescending(r => r.point.y).ToArray();

            List<Node> nodes = ProcessCollisions(hits);

            NodeLine = new NodeLine(x, verticalLine.x, verticalLine.y, nodes);
            PathLine = new PathLine(NodeLine);
        }

        public void BuildNodeEdge() {
            if (PreviousLineScanner != null) {
                foreach (var node in PreviousLineScanner.NodeLine.Nodes) {
                    if (!node.Vaild || node is EdgeNode) continue;
                    Node aboveNode = NodeLine.GetNodeAboveY(node.Position.y);

                    bool invaildEdgePlacement = NodeLine.Nodes.Exists(n =>
                        !(n is EdgeNode) &&
                        Mathf.Abs(n.Position.y - node.Position.y) < MIN_DISTANCE_BETWEEN_NODES
                    );

                    if (!invaildEdgePlacement) {
                        if (node is CeilingNode && !(aboveNode is EdgeNode)) continue;

                        EdgeNode edgeNode = new EdgeNode(LineIndex, new Vector2(x, node.Position.y), EdgeNode.Direction.Right, node is CeilingNode);
                        NodeLine.AddNode(edgeNode);
                    }
                }
            }
            if (NextLineScanner != null) {
                foreach (var node in NextLineScanner.NodeLine.Nodes) {
                    if (!node.Vaild || node is EdgeNode) continue;
                    Node aboveNode = NodeLine.GetNodeAboveY(node.Position.y);

                    bool invaildEdgePlacement = NodeLine.Nodes.Exists(n =>
                        !(n is EdgeNode) &&
                        Mathf.Abs(n.Position.y - node.Position.y) < MIN_DISTANCE_BETWEEN_NODES
                    );

                    if (!invaildEdgePlacement) {
                        if (node is CeilingNode && !(aboveNode is EdgeNode)) {
                            continue;
                        }

                        Node edgeNode = new EdgeNode(LineIndex, new Vector2(x, node.Position.y), EdgeNode.Direction.Left, node is CeilingNode);
                        NodeLine.AddNode(edgeNode);
                    }
                }
            }
        }

        public void BuildAirNodes() {
            foreach (var node in NodeLine.Nodes) {
                if (node.Vaild && node is EdgeNode edgeNode) {
                    EdgeNode.Direction direction = edgeNode.EdgeDirection;

                    LineScanner currentLineScanner = direction == EdgeNode.Direction.Left ? PreviousLineScanner : NextLineScanner;
                    LineScanner lastLineScanenr = currentLineScanner;
                    Dictionary<LineScanner, List<AirNode>> currentAirNodes = new Dictionary<LineScanner, List<AirNode>>();

                    int remaingScans = Mathf.CeilToInt(AIR_NODES_RANGE / metherPerLine);
                    bool breakLoop = false;
                    bool isEdgeAirNode = false;

                    while (currentLineScanner != null) {
                        Node overlappingNode = currentLineScanner.NodeLine.GetNodeAtY(node.Position.y);
                        if (overlappingNode != null && (overlappingNode is EdgeNode || overlappingNode is AirNode)) break;

                        AirNode airNode = new AirNode(currentLineScanner.LineIndex, new Vector2(currentLineScanner.x, node.Position.y), isEdgeAirNode);
                        if (!currentAirNodes.ContainsKey(currentLineScanner)) currentAirNodes.Add(currentLineScanner, new List<AirNode>());
                        currentAirNodes[currentLineScanner].Add(airNode);

                        lastLineScanenr = currentLineScanner;
                        currentLineScanner = direction == EdgeNode.Direction.Left
                            ? currentLineScanner.PreviousLineScanner
                            : currentLineScanner.NextLineScanner;

                        if (currentLineScanner == null) break;

                        Node aboveNode = currentLineScanner.NodeLine.GetNodeAboveY(node.Position.y, typeof(AirNode));
                        Node belowNode = currentLineScanner.NodeLine.GetNodeBelowY(node.Position.y, typeof(AirNode));

                        if (breakLoop) {
                            break;
                        } else if (remaingScans == 0) {
                            break;
                        } else if (aboveNode is GroundNode && belowNode is CeilingNode) {
                            break;
                        } else if (aboveNode is EdgeNode aboveEdgeNode && aboveEdgeNode.EdgeDirection != direction) {
                            isEdgeAirNode = true;
                            breakLoop = true;
                        } else if (belowNode is EdgeNode belowEdgeNode && belowEdgeNode.EdgeDirection != direction) {
                            isEdgeAirNode = true;
                            breakLoop = true;
                        }

                        remaingScans--;
                    }

                    if (currentLineScanner == null || remaingScans == 0) break;

                    foreach (var currentAirNode in currentAirNodes) {
                        foreach (var airNode in currentAirNode.Value) {
                            currentAirNode.Key.NodeLine.AddNode(airNode);
                        }
                    }
                }
            }
        }

        public void BuildPathLine() {
            List<PathNode> paths = new List<PathNode>();

            List<Node> vaildNodes = NodeLine.VaildNodes;
            PathLine previousNodeLine = PreviousLineScanner?.PathLine;

            for (int i = vaildNodes.Count - 1; i >= 0; i--) {
                Node node = vaildNodes[i];
                PathNode belowNode = PathLine.GetNodeBelowY(node.Position.y);


                if (node is GroundNode groundNode) {
                    PathNode pathNodeGround = new PathNode(node);
                    PathLine.AddNode(pathNodeGround);

                    PathNode previousNodeGround = previousNodeLine?.GetNodeAtY(node.Position.y, 1, typeof(GroundNode), typeof(EdgeNode));

                    if (previousNodeGround != null)
                        pathNodeGround.ConnectNode(previousNodeGround);

                    paths.Add(pathNodeGround);
                } else if (node is EdgeNode edgeNode) {
                    PathNode pathNodeEdge = new PathNode(node);
                    PathLine.AddNode(pathNodeEdge);

                    PathNode previousNodeEdge = previousNodeLine?.GetNodeAtY(node.Position.y, 1, typeof(GroundNode), typeof(AirNode));

                    if (previousNodeEdge != null)
                        pathNodeEdge.ConnectNode(previousNodeEdge);
                    if (belowNode?.Node is GroundNode || belowNode?.Node is EdgeNode)
                        pathNodeEdge.ConnectNode(belowNode);

                    paths.Add(pathNodeEdge);
                } else if (node is AirNode airNode) {
                    PathNode pathNodeAir = new PathNode(node);
                    PathLine.AddNode(pathNodeAir);

                    PathNode previousNode = previousNodeLine?.GetNodeAtY(node.Position.y, 1, typeof(AirNode), typeof(EdgeNode));

                    if (previousNode != null)
                        pathNodeAir.ConnectNode(previousNode);

                    if (belowNode?.Node is EdgeNode ||
                        belowNode?.Node is AirNode ||
                       (belowNode?.Node is GroundNode && airNode.IsEdgeAirNode)
                    ) pathNodeAir.ConnectNode(belowNode);

                    paths.Add(pathNodeAir);
                }
            }
        }

        private List<Node> ProcessCollisions(RaycastHit2D[] hits) {
            List<Node> nodes = new List<Node>();

            bool lastNodeGround = false;
            bool isFirst = true;

            HashSet<Collider2D> activeColliders = new HashSet<Collider2D>();

            foreach (var hit in hits) {
                Vector2 position = hit.point;

                bool isGround = hit.normal.y > 0;

                float spaceAbove = float.MaxValue;
                int id = hit.collider.GetInstanceID();

                bool valid = true;

                if (isGround) {
                    if (activeColliders.Count > 0) {
                        valid = false;
                    }

                    activeColliders.Add(hit.collider);
                } else {
                    if (activeColliders.Count != 1) {
                        valid = false;
                    }

                    activeColliders.Remove(hit.collider);
                }

                if (position.y >= bound.x || position.y <= bound.y) {
                    valid = false;
                } else if (!isFirst && isGround == lastNodeGround) {
                    valid = false;
                }

                if (nodes.Count > 0) {
                    Node lastNode = nodes[nodes.Count - 1];
                    spaceAbove = Mathf.Abs(lastNode.Position.y - position.y);
                }

                Node node = isGround
                    ? (Node)new GroundNode(id, LineIndex, valid, position, spaceAbove)
                    : (Node)new CeilingNode(id, LineIndex, valid, position);

                nodes.Add(node);

                if (valid) {
                    lastNodeGround = isGround;
                    isFirst = false;
                }
            }

            return nodes;
        }
    }
}
