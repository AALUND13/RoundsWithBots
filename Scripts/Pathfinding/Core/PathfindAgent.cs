using RoundsWithBots.Pathfinding.Nodes;

namespace RoundsWithBots.Pathfinding.Core {
    public class PathfindAgent {
        public PathfindRequest CurrentPathRequest;

        public void Pathfind(PathNode startNode, PathNode endNode, PathfindingOptions pathfindingOptions) {
            CurrentPathRequest = new PathfindRequest(startNode, endNode, pathfindingOptions);
            CurrentPathRequest.Pathfind();
        }
    }
}
