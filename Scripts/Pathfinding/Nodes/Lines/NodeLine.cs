using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoundsWithBots.Pathfinding.Nodes.Lines {
    public class NodeLine {
        public float X;
        public float StartY;
        public float EndY;

        public List<Node> Nodes;
        public List<Node> VaildNodes => Nodes.Where(n => n.Vaild).ToList();

        public NodeLine(float x, float startY, float endY, List<Node> nodes) {
            this.X = x;
            this.StartY = startY;
            this.EndY = endY;
            this.Nodes = nodes.OrderByDescending(n => n.Position.y).ToList();
        }

        public void AddNode(Node node) {
            int index = Nodes.FindIndex(n => n.Position.y < node.Position.y);

            if (index == -1)
                Nodes.Add(node);
            else
                Nodes.Insert(index, node);
        }

        public Node GetNodeAboveY(float y, params Type[] blacklistNodeType) {
            if (blacklistNodeType.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(blacklistNodeType)
                );
            }

            for (int i = Nodes.Count - 1; i >= 0; i--) {
                if (Nodes[i].Vaild && Nodes[i].Position.y > y && !blacklistNodeType.Contains(Nodes[i].GetType())) {
                    return Nodes[i];
                }
            }
            return null;
        }

        public Node GetNodeBelowY(float y, params Type[] blacklistNodeType) {
            if (blacklistNodeType.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(blacklistNodeType)
                );
            }

            for (int i = 0; i < Nodes.Count; i++) {
                if (Nodes[i].Vaild && Nodes[i].Position.y < y && !blacklistNodeType.Contains(Nodes[i].GetType())) {
                    return Nodes[i];
                }
            }
            return null;
        }

        public Node GetNodeAtY(float y, float minDist = 1f) {
            foreach (Node node in Nodes) {
                if (Mathf.Abs(node.Position.y - y) < minDist && node.Vaild)
                    return node;
            }
            return null;
        }


        public Node GetNodeAtY(float y, float minDist = 1f, params Type[] whitelistedNodeTypes) {
            if (whitelistedNodeTypes.Any(t => !t.IsSubclassOf(typeof(Node)))) {
                throw new ArgumentException(
                    "All types in blacklistNodeType must inherit from Node.",
                    nameof(whitelistedNodeTypes)
                );
            }

            foreach (Node node in Nodes) {
                if (Mathf.Abs(node.Position.y - y) < minDist && node.Vaild && (whitelistedNodeTypes.Length == 0 || whitelistedNodeTypes.Contains(node.GetType()))) {
                    return node;
                }
            }

            return null;
        }
    }
}
