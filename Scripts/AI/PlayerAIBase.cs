using RoundsWithBots.AI.Movement;
using RoundsWithBots.Pathfinding.Core;
using RoundsWithBots.Pathfinding.Nodes;
using UnityEngine;

namespace RoundsWithBots.AI {
    public class PlayerAIBase : MonoBehaviour {
        public PathfindAgent Agent;
        public PlayerAPI PlayerAPI;

        private void Awake() {
            Agent = new PathfindAgent();
            PlayerAPI = GetComponentInParent<PlayerAPI>();

            gameObject.AddComponent<AIAttackModule>();
            gameObject.AddComponent<AIBlockModule>();
            gameObject.AddComponent<AIPathfindModule>();
            gameObject.AddComponent<AIMovementModule>();
        }

        public bool IsNodeVaild(Node node) {
            bool isCeilingEdgeNode = node is EdgeNode edgeNode && edgeNode.IsCeilingEdge;
            bool isAirNode = node is AirNode;

            return !isAirNode && !isCeilingEdgeNode;
        }


        public float GetRadius(GameObject obj) {
            return obj.transform.parent.localScale.y / 2;
        }

        public float GetSurfaceDistanceToPosition(Vector2 position) {
            return Vector2.Distance(PlayerAPI.PlayerPosition(), position) - GetRadius(PlayerAPI.gameObject);
        }
    }
}
