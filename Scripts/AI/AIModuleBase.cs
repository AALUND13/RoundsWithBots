using RoundsWithBots.Pathfinding.Nodes;
using UnityEngine;

namespace RoundsWithBots.AI {
    [RequireComponent(typeof(PlayerAIBase))]
    public class AIModuleBase : MonoBehaviour {
        protected PlayerAIBase playerAIBase;
        protected PlayerAPI playerAPI;

        protected readonly int NonPlayerLayerMask =
            LayerMask.GetMask("Default", "IgnorePlayer");


        private void Awake() {
            playerAIBase = GetComponent<PlayerAIBase>();
            playerAPI = GetComponentInParent<PlayerAPI>();
            OnAwake();
        }
        protected virtual void OnAwake() { }



        public float GetRadius(Transform transform) {
            return transform.localScale.y / 2;
        }

        public float GetSurfaceDistanceToPosition(Vector2 position) {
            return Vector2.Distance(playerAPI.PlayerPosition(), position) - GetRadius(playerAPI.transform);
        }

        public float GetSurfaceDistanceToObject(Transform targetTransform) {
            return Vector2.Distance(playerAPI.PlayerPosition(), targetTransform.position) - (GetRadius(playerAPI.transform) + GetRadius(targetTransform));
        }

        public bool CanSeeTarget(Vector2 from, Vector2 to) {
            bool canSeeTarget = true;
            RaycastHit2D[] array = Physics2D.RaycastAll(from, (to - from).normalized, Vector2.Distance(from, to), NonPlayerLayerMask);

            foreach (RaycastHit2D ray in array) {
                canSeeTarget = false;
                break;
            }

            return canSeeTarget;
        }

        public bool IsNodeVaild(Node node) {
            bool isCeilingEdgeNode = node is EdgeNode edgeNode && edgeNode.IsCeilingEdge;
            bool isAirNode = node is AirNode;

            return !isAirNode && !isCeilingEdgeNode;
        }
    }
}
