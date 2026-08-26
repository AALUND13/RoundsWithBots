using BepInEx.Logging;
using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Utils;
using RoundsWithBots.Utils.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoundsWithBots.Pathfinding.Core {
    public class PathfindRequest {
        private const int MAX_ITERATIONS = 10000;

        public PathNode StartNode;
        public PathNode EndNode;
        public PathfindingOptions PathfindingOptions;

        public List<VisitedNode> Path = new List<VisitedNode>();
        public bool PathfindFinishes { get; private set; } = false;

        private readonly Dictionary<PathNode, VisitedNode> visitedNodes = new Dictionary<PathNode, VisitedNode>();
        private readonly HashSet<PathNode> closedSet = new HashSet<PathNode>();
        private readonly DaryHeap<VisitedNode> openSet = new DaryHeap<VisitedNode>(4);

        public PathfindRequest(PathNode startNode, PathNode endNode, PathfindingOptions penaltyOption) {
            StartNode = startNode;
            EndNode = endNode;
            PathfindingOptions = penaltyOption;

            VisitedNode start = new VisitedNode(StartNode);
            start.G = 0;
            start.H = start.GetDistance(EndNode);

            visitedNodes.Add(StartNode, start);
            openSet.Add(start);
        }

        public void CondensePath() {
            if (Path.Count <= 2) return;

            Type lastNodeType = Path[0].PathNode.Node.GetType();
            List<VisitedNode> condensedPath = new List<VisitedNode>();
            condensedPath.Add(Path[0]);


            for (int i = 1; i < Path.Count - 1; i++) {
                VisitedNode prevNode = Path[i - 1];
                VisitedNode currentNode = Path[i];
                VisitedNode nextNode = Path[i + 1];

                float straightness = MathUtils.CalculatePointsStraightness(prevNode.PathNode.Node.Position, currentNode.PathNode.Node.Position, nextNode.PathNode.Node.Position);
                if (straightness > 0.1f || currentNode.PathNode.Node.GetType() != lastNodeType) {
                    condensedPath.Add(currentNode);
                    lastNodeType = currentNode.PathNode.Node.GetType();
                }
            }

            condensedPath.Add(Path[Path.Count - 1]);
            Path = condensedPath;
        }

        public void Pathfind() {
            if (PathfindFinishes) return;

            int iterations = 0;
            while (openSet.Count > 0) {
                if (++iterations > MAX_ITERATIONS) {
                    LoggerUtils.LogWarn(
                        $"A* exceeded {MAX_ITERATIONS} iterations!"
                    );
                    break;
                }

                VisitedNode current = openSet.RemoveFirst();
                closedSet.Add(current.PathNode);

                if (current.PathNode == EndNode) {
                    Path = BuildPath(current);
                    break;
                }

                foreach (PathNode neighbour in current.PathNode.ConnetedNodes) {
                    if (!visitedNodes.TryGetValue(neighbour, out VisitedNode neighbourVisited)) {
                        neighbourVisited = new VisitedNode(neighbour);
                        neighbourVisited.H = neighbourVisited.GetDistance(EndNode);
                        visitedNodes.Add(neighbour, neighbourVisited);
                    }

                    var costToNeighbour = current.G + current.GetDistance(neighbour);


                    if (neighbour.Node is GroundNode groundNode) {
                        if (groundNode.spaceAbove < PathfindingOptions.MinSpaceRequire) {
                            continue;
                        }
                    }

                    if (neighbour.Node.LineIndex == current.PathNode.Node.LineIndex && neighbour.Node.Position.y > current.PathNode.Node.Position.y) {
                        if (
                             (neighbour.Node is AirNode || (neighbour.Node is EdgeNode edgeNode && edgeNode.IsCeilingEdge)) &&
                            !(current.PathNode.Node is EdgeNode)
                        ) {
                            float verticalGap = current.JumpDist + Mathf.Abs(neighbour.Node.Position.y - current.PathNode.Node.Position.y);
                            neighbourVisited.JumpDist = verticalGap;

                            if (verticalGap > PathfindingOptions.JumpMeters) continue;
                        }
                    }

                    float newG = costToNeighbour + neighbourVisited.GetPenalty(PathfindingOptions);
                    if (newG >= neighbourVisited.G) continue;


                    neighbourVisited.G = newG;
                    neighbourVisited.Parent = current;

                    if (openSet.Contains(neighbourVisited)) {
                        openSet.UpdateItem(neighbourVisited);
                    } else {
                        openSet.Add(neighbourVisited);
                    }
                }
            }

            PathfindFinishes = true;
        }

        public List<VisitedNode> BuildPath(VisitedNode visitedNode) {
            List<VisitedNode> paths = new List<VisitedNode>();
            HashSet<PathNode> visited = new HashSet<PathNode>();

            VisitedNode current = visitedNode;

            while (current != null) {
                if (!visited.Add(current.PathNode)) {
                    LoggerUtils.Log(LogLevel.Error, "Cycle detected in parent chain!");
                    return new List<VisitedNode>();
                }

                paths.Add(current);
                current = current.Parent;
            }

            paths.Reverse();
            return paths;
        }
    }
}
