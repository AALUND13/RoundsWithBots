using RoundsWithBots.Pathfinding.Core;
using RoundsWithBots.Pathfinding.Nodes;
using RoundsWithBots.Utils.Debug;
using UnityEngine;

namespace RoundsWithBots.AI.Movement {
    [RequireComponent(typeof(AIPathfindModule))]
    public class AIMovementModule : AIModuleBase {
        [Header("Pathfinding Thresholds")]
        public float TargetNodeDistanceThreshold = 1.5f;
        public float HorizontalEpsilon = 0.025f;
        public float FastFallThreshold = 2.5f;
        public float WallHugDistance = 5f;

        private AIPathfindModule pathfindingHandler;
        private bool isJumping = false;


        protected override void OnAwake() {
            pathfindingHandler = GetComponent<AIPathfindModule>();
        }

        private void Update() {
            PathfindPlayer();

            playerAPI.data.input.jumpIsPressed = isJumping;
            playerAPI.data.input.jumpWasPressed = isJumping;
        }

        private void LateUpdate() {
            isJumping = false;
        }


        private void PathfindPlayer() {
            if (pathfindingHandler.TargetNode != null && pathfindingHandler.StartNode != null && pathfindingHandler.EndNode != null) {
                // Determine horizontal direction
                Vector2 direction = pathfindingHandler.TargetNode.Node.Position - (Vector2)playerAPI.PlayerPosition();
                float horiz = Mathf.Abs(direction.x) < HorizontalEpsilon
                    ? 0f
                    : Mathf.Sign(direction.x);

                // Fast Fall
                bool withinFastFallDistance = direction.y < -GetRadius(playerAPI.transform) - FastFallThreshold;
                bool canFastFall = withinFastFallDistance && pathfindingHandler.TargetNode.Node is GroundNode;

                // Movement
                Vector2 moveDirection = new Vector2(horiz, canFastFall ? -1 : 0);
                if (pathfindingHandler.TargetNode.Node.Position.y > playerAPI.PlayerPosition().y) {
                    float dir = AttemptWallGrab();
                    if (dir != 0) {
                        moveDirection.x = dir;
                    }
                }

                if (direction.y > 0) {
                    isJumping = true;
                }
                playerAPI.Move(moveDirection);

                // Debugging
                if (playerAPI != null && DebugVisualizer.EnableCategory.HasFlag(DebugVisualizer.VisualizerCategory.Bots)) {
                    DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.Bots, $"Bot_StartNode_{playerAPI.player.playerID}", Color.green, pathfindingHandler.StartNode.Node.Position);
                    DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.Bots, $"Bot_NextNode_{playerAPI.player.playerID}", Color.yellow, pathfindingHandler.TargetNode.Node.Position);
                    DebugVisualizer.RenderDebugSphere(DebugVisualizer.VisualizerCategory.Bots, $"Bot_EndNode_{playerAPI.player.playerID}", Color.red, pathfindingHandler.EndNode.Node.Position);

                    if (playerAIBase.Agent.CurrentPathRequest.Path.Count > 1) {
                        VisitedNode currentNode = playerAIBase.Agent.CurrentPathRequest.Path[0];
                        for (int i = 1; i < playerAIBase.Agent.CurrentPathRequest.Path.Count; i++) {
                            VisitedNode candidate = playerAIBase.Agent.CurrentPathRequest.Path[i];

                            if (playerAIBase.IsNodeVaild(candidate.PathNode.Node)) {
                                DebugVisualizer.RenderDebugLine(DebugVisualizer.VisualizerCategory.Bots, $"Bot_Path_Index_{i}_{playerAIBase.PlayerAPI.player.playerID}", Color.blue, currentNode.PathNode.Node.Position, candidate.PathNode.Node.Position);
                                currentNode = candidate;
                            }
                        }
                    }
                }
            }
        }

        private float AttemptWallGrab() {
            RaycastHit2D right = Physics2D.Raycast(playerAIBase.PlayerAPI.PlayerPosition(), Vector2.right, (transform.parent.localScale.x / 2) + WallHugDistance, NonPlayerLayerMask);
            RaycastHit2D left = Physics2D.Raycast(playerAIBase.PlayerAPI.PlayerPosition(), Vector2.left, (transform.parent.localScale.x / 2) + WallHugDistance, NonPlayerLayerMask);

            bool hasLeft = left.collider != null;
            bool hasRight = right.collider != null;

            if (!hasLeft && !hasRight)
                return 0;
            else if (hasLeft ^ hasRight)
                return hasLeft ? -1 : 1;

            bool leftCloser =
                Mathf.Abs(left.point.x - pathfindingHandler.TargetNode.Node.Position.x) <
                Mathf.Abs(right.point.x - pathfindingHandler.TargetNode.Node.Position.x);

            return leftCloser ? -1 : 1;
        }
    }
}
