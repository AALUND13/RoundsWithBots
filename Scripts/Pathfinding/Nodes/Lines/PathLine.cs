using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes.Lines {
    public class PathLine {
        public NodeLine NodeLine;
        public List<PathNode> Paths = new List<PathNode>();

        public PathLine(NodeLine nodeLine) {
            NodeLine = nodeLine;
        }

        public void AddNode(PathNode node) {
            int index = Paths.FindIndex(n => n.Node.Position.y < node.Node.Position.y);

            if (index == -1)
                Paths.Add(node);
            else
                Paths.Insert(index, node);
        }

        public PathNode GetNodeAboveY(float y, params Type[] blacklistNodeType) {
            if (blacklistNodeType.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(blacklistNodeType)
                );
            }

            for (int i = Paths.Count - 1; i >= 0; i--) {
                if (Paths[i].Node.Vaild && Paths[i].Node.Position.y > y && !blacklistNodeType.Contains(Paths[i].Node.GetType())) {
                    return Paths[i];
                }
            }
            return null;
        }

        public PathNode GetNodeBelowY(float y, params Type[] blacklistNodeType) {
            if (blacklistNodeType.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(blacklistNodeType)
                );
            }

            for (int i = 0; i < Paths.Count; i++) {
                if (Paths[i].Node.Vaild && Paths[i].Node.Position.y < y && !blacklistNodeType.Contains(Paths[i].Node.GetType())) {
                    return Paths[i];
                }
            }
            return null;
        }

        public PathNode GetNodeAtY(float y, float minDist = 1f) {
            foreach (PathNode node in Paths) {
                if (Mathf.Abs(node.Node.Position.y - y) < minDist && node.Node.Vaild)
                    return node;
            }
            return null;
        }


        public PathNode GetNodeAtY(float y, float minDist = 1f, params Type[] whitelistedNodeTypes) {
            if (whitelistedNodeTypes.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(whitelistedNodeTypes)
                );
            }

            foreach (PathNode node in Paths) {
                if (Mathf.Abs(node.Node.Position.y - y) < minDist && node.Node.Vaild && (whitelistedNodeTypes.Length == 0 || whitelistedNodeTypes.Contains(node.Node.GetType()))) {
                    return node;
                }
            }

            return null;
        }
    }
}
