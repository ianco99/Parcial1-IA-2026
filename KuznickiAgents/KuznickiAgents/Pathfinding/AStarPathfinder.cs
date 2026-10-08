using System;
using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
    public class AStarPathfinder<TNodeType> : Pathfinder<TNodeType> where TNodeType : IPathfindingNode
    {
        private static readonly (int x, int y)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };

        private readonly Dictionary<(int x, int y), TNodeType> nodesByCoordinate =
            new Dictionary<(int x, int y), TNodeType>();

        public AStarPathfinder(ICollection<TNodeType> nodes) : base(nodes)
        {
            foreach (TNodeType node in nodes)
            {
                nodesByCoordinate[node.GetCoordinate()] = node;
            }
        }

        protected override int Distance(TNodeType A, TNodeType B)
        {
            (int x, int y) a = A.GetCoordinate();
            (int x, int y) b = B.GetCoordinate();
            (int width, int height) dimensions = A.GetDimensions();
            return Math.Abs(b.x - a.x) * dimensions.width + Math.Abs(b.y - a.y) * dimensions.height;
        }

        protected override ICollection<TNodeType> GetNeighbors(TNodeType node)
        {
            List<TNodeType> neighbors = new List<TNodeType>(directions.Length);
            (int x, int y) coordinate = node.GetCoordinate();

            foreach ((int x, int y) direction in directions)
            {
                if (nodesByCoordinate.TryGetValue((coordinate.x + direction.x, coordinate.y + direction.y),
                        out TNodeType neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        protected override bool IsBlocked(TNodeType node)
        {
            return node.IsBlocked();
        }

        protected override int MoveToNeighborCost(TNodeType A, TNodeType B)
        {
            return Distance(A, B) * B.GetCost();
        }

        protected override bool NodesEquals(TNodeType A, TNodeType B)
        {
            return A.GetCoordinate() == B.GetCoordinate();
        }
    }
}
