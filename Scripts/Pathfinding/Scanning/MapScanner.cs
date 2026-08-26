using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Utils.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RoundsWithBots.Pathfinding.Scanning.MapScanner;

namespace RoundsWithBots.Pathfinding.Scanning {
    sealed public class MapScanner : MonoBehaviour {
        public class ClosestNodeResult {
            public PathNode Node;
            public float DistanceSquared = float.MaxValue;
        }

        public static MapScanner Instance { get; private set; }
        public static float MapSize => MapManager.instance?.currentMap?.Map?.size ?? 20;

        public LineScanner[] LineScanners { get; private set; }

        public int layerMask = LayerMask.GetMask("Default", "IgnorePlayer");

        public float ScanlineSpacing = 0.95f;
        public float MapWidthZeroScale = 3.556f;
        public float MapHeightZeroScale = 2.0f;
        public float RayYOffsetMultiplier = 1.5f;

        private float actualScanlineSpacing = 0;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            } else {
                Instance = this;
            }
        }

        private void FixedUpdate() {
            ScanMap();
        }

        private void Update() {
            if (!DebugVisualizer.EnableCategory.HasFlag(DebugVisualizer.VisualizerCategory.MapScanner)) {
                return;
            }

            List<PathNode> vistedNodes = new List<PathNode>();
            foreach (var scanLine in LineScanners) {
                if (scanLine == null || scanLine.NodeLine == null) continue;

                // Draw the nodes
                foreach (var node in scanLine.NodeLine.Nodes) {
                    if (node.Vaild) {
                        if (node is GroundNode) {
                            DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.MapScanner, $"Node [Ground] | x: {node.Position.x} y: {node.Position.y}", Color.green, node.Position, 0.25f);
                        } else if (node is CeilingNode) {
                            DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.MapScanner, $"Node [Ceiling] | x: {node.Position.x} y: {node.Position.y}", Color.red, node.Position, 0.25f);
                        } else if (node is EdgeNode) {
                            DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.MapScanner, $"Node [Edge] | x: {node.Position.x} y: {node.Position.y}", Color.cyan, node.Position, 0.25f);
                        } else if (node is AirNode) {
                            DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.MapScanner, $"Node [Air] | x: {node.Position.x} y: {node.Position.y}", Color.yellow, node.Position, 0.25f);
                        }
                    }
                }

                foreach (var path in scanLine.PathLine.Paths) {
                    foreach (var node in path.ConnetedNodes) {
                        if (vistedNodes.Contains(node)) continue;

                        DebugVisualizer.RenderDebugLine(DebugVisualizer.VisualizerCategory.MapScanner, $"Path from: {path.Node.Position} to {node.Node.Position}", Color.black, path.Node.Position, node.Node.Position);
                    }

                    vistedNodes.Add(path);
                }
            }
        }

        public void ScanMap() {
            Profiler.Start("Map Scanning");

            float mapWidth = MapWidthZeroScale * MapSize;
            int numLineScanners = Mathf.CeilToInt(mapWidth / ScanlineSpacing);
            actualScanlineSpacing = mapWidth / (numLineScanners - 1);
            LineScanners = new LineScanner[numLineScanners];

            // Step 1: Create LineScanners
            Profiler.Start("Create LineScanners");

            for (int i = 0; i < LineScanners.Length; i++) {
                float x = -MapSize * MapWidthZeroScale / 2 + i * actualScanlineSpacing;
                float scaleY = MapSize * MapHeightZeroScale / 2;

                LineScanners[i] = new LineScanner(
                    x,
                    i,
                    actualScanlineSpacing,
                    new Vector2(
                        scaleY * RayYOffsetMultiplier,
                        -scaleY
                    ),
                    new Vector2(scaleY, -scaleY),
                    layerMask
                );
            }

            Profiler.End("Create LineScanners");


            // Step 2: Initialize neighbor scanners
            Profiler.Start("Initialize Neighbors");

            for (int i = 0; i < LineScanners.Length; i++) {
                LineScanner previous = i > 0
                    ? LineScanners[i - 1]
                    : null;

                LineScanner next = i < LineScanners.Length - 1
                    ? LineScanners[i + 1]
                    : null;

                LineScanners[i].InitializeNeighborScanners(previous, next);
            }

            Profiler.End("Initialize Neighbors");


            // Building
            Profiler.Start("Building");


            // Step 3: Build node lines
            Profiler.Start("Build Node Lines");
            foreach (var lineScanner in LineScanners) {
                lineScanner.BuildNodeLine();
            }
            Profiler.End("Build Node Lines");

            // Step 4: Build edge nodes
            Profiler.Start("Build Edge Nodes");
            foreach (var lineScanner in LineScanners) {
                lineScanner.BuildNodeEdge();
            }
            Profiler.End("Build Edge Nodes");

            // Step 5: Build air nodes
            Profiler.Start("Build Air Nodes");

            int count = LineScanners.Length;
            int mid = count / 2;

            for (int i = 0; i < mid; i++) {
                LineScanners[i].BuildAirNodes();
            }

            for (int i = count - 1; i >= mid; i--) {
                LineScanners[i].BuildAirNodes();
            }
            Profiler.End("Build Air Nodes");


            // Step 6: Build paths
            Profiler.Start("Build Path Lines");
            foreach (var lineScanner in LineScanners) {
                lineScanner.BuildPathLine();
            }
            Profiler.End("Build Path Lines");


            Profiler.End("Building");
            Profiler.End("Map Scanning");
            Profiler.Log();
        }

        public LineScanner GetLineScanner(int index) {
            if (LineScanners == null || index < 0 || index >= LineScanners.Length) return null;
            return LineScanners[index];
        }

        public LineScanner GetLineScannerAtX(float x) {
            for (int i = 0; i < LineScanners.Length; i++) {
                LineScanner lineScanner = LineScanners[i];
                if (lineScanner.x >= x) {
                    return lineScanner;
                }
            }
            return null;
        }

        public PathNode[] GetVaildNodeInRange(Vector2 target, Func<PathNode, LineScanner, bool> selector, float range = 1) {
            if (LineScanners == null) return new PathNode[0];

            List<PathNode> pathNodes = new List<PathNode>();

            for (int i = 0; i < LineScanners.Length; i++) {
                LineScanner lineScanner = LineScanners[i];
                if (lineScanner.x >= target.x) {
                    int actualRange = Mathf.CeilToInt((range - 1) / actualScanlineSpacing);
                    int startIndex = Mathf.Max(i - actualRange, 0);
                    int endIndex = Mathf.Min(i + actualRange, LineScanners.Length - 1);

                    for (int x = startIndex; x <= endIndex; x++) {
                        LineScanner lineScannerScan = LineScanners[x];

                        foreach (var pathNode in lineScannerScan.PathLine.Paths) {
                            if (selector.Invoke(pathNode, lineScannerScan)) pathNodes.Add(pathNode);
                        }
                    }

                    break;
                }
            }

            return pathNodes.ToArray();
        }

        public ClosestNodeResult GetClosestPathNode(Vector2 target, Func<LineScanner, PathNode, bool> selector) {
            ClosestNodeResult closestNodeResult = new ClosestNodeResult();
            int centerIndex = 0;

            if (LineScanners == null || LineScanners.Length == 0) {
                return closestNodeResult;
            }

            for (int i = 0; i < LineScanners.Length; i++) {
                LineScanner lineScanner = LineScanners[i];
                if (lineScanner.x >= target.x) {
                    centerIndex = i;
                    closestNodeResult = GetClosetPathNodeInLine(target, lineScanner, selector);
                    break;
                }
            }

            int left = centerIndex - 1;
            int right = centerIndex + 1;

            bool leftDone = false;
            bool rightDone = false;

            while (!leftDone && !rightDone) {
                if (!leftDone) {
                    if (left < 0) {
                        leftDone = true;
                    } else {
                        LineScanner lineScanner = LineScanners[left];
                        float distanceInX = lineScanner.x - target.x;
                        float distanceInXSquared = distanceInX * distanceInX;

                        ClosestNodeResult nodeResult = GetClosetPathNodeInLine(target, lineScanner, selector);
                        if (nodeResult.DistanceSquared < closestNodeResult.DistanceSquared) {
                            closestNodeResult = nodeResult;
                        }

                        if (distanceInXSquared > closestNodeResult.DistanceSquared) {
                            leftDone = true;
                        }

                        left--;
                    }
                }

                if (!rightDone) {
                    if (right >= LineScanners.Length) {
                        rightDone = true;
                    } else {
                        LineScanner lineScanner = LineScanners[right];
                        float distanceInX = lineScanner.x - target.x;
                        float distanceInXSquared = distanceInX * distanceInX;

                        ClosestNodeResult nodeResult = GetClosetPathNodeInLine(target, lineScanner, selector);
                        if (nodeResult.DistanceSquared < closestNodeResult.DistanceSquared) {
                            closestNodeResult = nodeResult;
                        }

                        if (distanceInXSquared > closestNodeResult.DistanceSquared) {
                            rightDone = true;
                        }

                        right++;
                    }
                }
            }

            return closestNodeResult;
        }

        public ClosestNodeResult GetClosetPathNodeInLine(Vector2 target, LineScanner lineScanner, Func<LineScanner, PathNode, bool> selector) {
            ClosestNodeResult closestNodeResult = new ClosestNodeResult();

            foreach (var pathNode in lineScanner.PathLine.Paths) {
                Node ceilingNode = lineScanner.NodeLine.GetNodeBelowY(pathNode.Node.Position.y);
                bool isBelowCeiling = ceilingNode != null &&
                                      ceilingNode is CeilingNode &&
                                      ceilingNode.Position.y > target.y;

                if (pathNode.Node is GroundNode && isBelowCeiling)
                    continue;

                foreach (PathNode spanNode in pathNode.GetVerticalNodes()) {
                    float distanceSquared = (target - spanNode.Node.Position).sqrMagnitude;

                    if (distanceSquared < closestNodeResult.DistanceSquared &&
                        (selector?.Invoke(lineScanner, spanNode) ?? true)) {

                        closestNodeResult.Node = spanNode;
                        closestNodeResult.DistanceSquared = distanceSquared;
                    }
                }
            }

            return closestNodeResult;
        }
    }
}
