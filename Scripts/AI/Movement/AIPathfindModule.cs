using RoundsWithBots.Pathfinding.Core;
using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Pathfinding.Scanning;
using RoundsWithBots.Utils.Debug;
using BepInEx.Logging;
using System.Linq;
using UnityEngine;

namespace RoundsWithBots.AI.Movement {
    public class AIPathfindModule : AIModuleBase {
        [Header("Reference")]
        public Vector2 targetPosition;

        [Header("Pathfinding Thresholds")]
        public float TargetNodeDistanceThreshold = 1.5f;

        [Header("Pathfinding Options")]
        public float PathfindTimeout = 2f;
        public float PathfindTargetDistanceThreshold = 3f;

        public float MinDistanceToPlayer = 4f;
        public float MaxDistanceToPlayer = 15f;

        public float DownwardNodesDistanceThreshold = 1f;

        public float GroundNodeDistanceThreshold = 2f;
        public float GroundNodeTime = 0.3f;
        public float GroundNodeCooldownTime = 0.5f;

        [Header("Pathfinding Penalty Options")]
        public float JumpPenaltyPow = 4f;
        public float JumpMeters = 6f;
        public float AirNodePenaltyMult = 0f;

        public Vector2 StartPlayerPosition =>
            new Vector2(playerAPI.PlayerPosition().x, playerAPI.PlayerPosition().y - (playerAPI.transform.localScale.y / 2));


        // Pathfinding
        public PathNode StartNode;
        public PathNode TargetNode;
        public PathNode EndNode;

        // Ground Pathfinding Optimization
        private Vector2 lastGroundPathfindTargetPos;
        private float lastGroundPathfindTime;

        private PathNode groundTargetNode;
        private float groundNodeCooldown;
        private float groundNodeTime;
        private bool moveToGroundNode;


        private void Update() {
            if (playerAPI.GetOtherPlayer() == null) {
                PathNode groundNode = GetClosestGroundNode(StartPlayerPosition);
                targetPosition = groundNode?.Node.Position ?? StartPlayerPosition;
            } else {
                Vector2 otherPlayerPosition = new Vector2(
                    playerAPI.OtherPlayerPosition().x,
                    playerAPI.OtherPlayerPosition().y -
                    (playerAPI.GetOtherPlayer().transform.localScale.y / 2)
                );

                if (!WithinAttackingDistance(targetPosition, otherPlayerPosition)) {
                    PathNode attackingNode = GetAttackingNode(otherPlayerPosition);
                    targetPosition = attackingNode?.Node.Position ?? otherPlayerPosition;
                }
            }

            if (targetPosition == null) {
                return;
            }

            TargetNode = GetTargetNode();
        }

        #region Tarrgeting

        public PathNode GetClosestGroundNode(Vector2 target) {
            PathNode[] nodes = MapScanner.Instance.GetVaildNodeInRange(target, (node, scanner) => {
                return node.Node is GroundNode;
            }, MaxDistanceToPlayer);

            if (nodes.Length > 0) {
                PathNode closestNode = null;
                float closestDistance = float.MaxValue;

                foreach (var node in nodes) {
                    float distance = (target - node.Node.Position).sqrMagnitude;
                    if (distance < closestDistance) {
                        closestNode = node;
                        closestDistance = distance;
                    }
                }

                return closestNode;
            } else {
                return null;
            }
        }

        public PathNode GetAttackingNode(Vector2 target) {
            PathNode[] nodes = MapScanner.Instance.GetVaildNodeInRange(target, (node, scanner) => {
                return node.Node is GroundNode && WithinAttackingDistance(new Vector2(node.Node.Position.x, node.Node.Position.y + GetRadius(transform)), target);
            }, MaxDistanceToPlayer);

            if (nodes.Length > 0) {
                PathNode closestNode = null;
                float closestDistance = float.MaxValue;

                foreach (var node in nodes) {
                    float distanceToTarget = (target - node.Node.Position).sqrMagnitude;
                    float distanceToBot = (StartPlayerPosition - node.Node.Position).sqrMagnitude;
                    float combineDistance = distanceToTarget + distanceToBot;

                    if (combineDistance < closestDistance) {
                        closestNode = node;
                        closestDistance = combineDistance;
                    }
                }

                return closestNode;
            } else {
                return null;
            }
        }

        public bool WithinAttackingDistance(Vector2 position, Vector2 target) {
            float distance = Vector2.Distance(position, target);
            return distance > MinDistanceToPlayer
                              && distance < MaxDistanceToPlayer
                              && CanSeeTarget(position, target);
        }

        #endregion
        #region Pathfinding

        private PathNode GetTargetNode() {
            bool hasTimeout =
                Time.time - lastGroundPathfindTime > PathfindTimeout;

            bool hasTargetMoved =
                Vector2.Distance(lastGroundPathfindTargetPos, targetPosition)
                > PathfindTargetDistanceThreshold;

            bool hasReachedTarget =
                TargetNode != null &&
                Vector2.Distance(
                    playerAPI.PlayerPosition(),
                    TargetNode.Node.Position
                ) < TargetNodeDistanceThreshold;


            PathNode nextNode = TargetNode;
            if (hasReachedTarget || hasTargetMoved || hasTimeout) {
                nextNode = AttemptPathfindToTarget(targetPosition)?.PathNode;
                if (nextNode?.Node is EdgeNode) {
                    PathNode[] connectedNodes =
                        nextNode.ConnetedNodes
                            .Where(n => n.Node is GroundNode)
                            .ToArray();

                    PathNode closestGroundNode =
                        connectedNodes
                            .OrderBy(n => Vector2.Distance(
                                n.Node.Position,
                                nextNode.Node.Position
                            ))
                            .FirstOrDefault();

                    if (closestGroundNode != null &&
                        (groundTargetNode == null || hasTimeout || hasTargetMoved)) {

                        groundTargetNode = closestGroundNode;

                        lastGroundPathfindTime = Time.time;
                        lastGroundPathfindTargetPos = targetPosition;
                    }
                } else if (groundTargetNode != null && (hasTimeout || hasTargetMoved)) {
                    groundTargetNode = null;
                    moveToGroundNode = false;
                }
            }

            // Ground-node movement logic
            if (groundTargetNode != null &&
                GetSurfaceDistanceToPosition(groundTargetNode.Node.Position)
                    < GroundNodeDistanceThreshold &&
                Time.time > groundNodeCooldown &&
                !moveToGroundNode) {

                groundNodeTime = Time.time + GroundNodeTime;
                moveToGroundNode = true;

            } else if (Time.time > groundNodeTime && moveToGroundNode) {
                groundNodeCooldown = Time.time + GroundNodeCooldownTime;
                groundTargetNode = null;
                moveToGroundNode = false;
            }

            PathNode result =
                moveToGroundNode
                    ? groundTargetNode ?? nextNode
                    : nextNode;

            return result;
        }

        private VisitedNode AttemptPathfindToTarget(Vector2 target) {
            LineScanner centerLineScanner = MapScanner.Instance.GetLineScannerAtX(StartPlayerPosition.x);
            Node abovePlayer = centerLineScanner.NodeLine.GetNodeAboveY(
                StartPlayerPosition.y,
                typeof(AirNode),
                typeof(GroundNode),
                typeof(EdgeNode)
            );

            StartNode = MapScanner.Instance.GetClosestPathNode(StartPlayerPosition, (scanner, pathNode) => {
                Node node = pathNode.Node;

                if (abovePlayer != null && node.Position.y >= abovePlayer.Position.y)
                    return false;
                if (node is GroundNode groundNode)
                    return groundNode.spaceAbove > playerAPI.transform.localScale.y;

                return true;
            }).Node;
            EndNode = MapScanner.Instance.GetClosestPathNode(target, (scanner, pathNode) => {
                Node node = pathNode.Node;

                if (node is GroundNode groundNode)
                    return groundNode.spaceAbove > playerAPI.transform.localScale.y;

                return true;
            }).Node;

            if (StartNode == null || EndNode == null) {
                return null;
            }

            playerAIBase.Agent.Pathfind(
                StartNode,
                EndNode,
                new PathfindingOptions(
                    // Penalty Options
                    JumpPenaltyPow,
                    AirNodePenaltyMult,
                    JumpMeters,

                    // Other Options
                    playerAPI.transform.localScale.y
                )
            );

            playerAIBase.Agent.CurrentPathRequest.CondensePath();

            if (playerAIBase.Agent.CurrentPathRequest.Path.Count > 0) {
                VisitedNode result = GetVaildNode(playerAIBase.Agent.CurrentPathRequest);

                return result;
            }

            return null;
        }

        private VisitedNode GetVaildNode(PathfindRequest pathRequest) {
            VisitedNode targetNode = pathRequest.Path[0];

            for (int i = 1; i < pathRequest.Path.Count; i++) {
                VisitedNode candidate = pathRequest.Path[i];
                VisitedNode lastNode = pathRequest.Path[i - 1];

                bool isPathGoingDown = candidate.PathNode.Node.Position.y < lastNode.PathNode.Node.Position.y;
                float distanceInX = Mathf.Abs(StartPlayerPosition.x - candidate.PathNode.Node.Position.x);

                if (IsNodeVaild(candidate.PathNode.Node)) {
                    bool discardNode = (candidate.PathNode.Node is GroundNode || (candidate.PathNode.Node is EdgeNode edgeNode && edgeNode.IsCeilingEdge)) && isPathGoingDown && distanceInX < DownwardNodesDistanceThreshold;
                    if (discardNode) continue;

                    targetNode = candidate;
                    break;
                }
            }

            return targetNode;
        }

        #endregion
    }
}
