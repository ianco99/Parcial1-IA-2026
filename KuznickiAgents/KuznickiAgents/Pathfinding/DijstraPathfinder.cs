using System;
using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
    public class DijstraPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : IPathfindingNode
    {
        private static readonly (int x, int y)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };

        private readonly Dictionary<(int x, int y), NodeType> nodesByCoordinate =
            new Dictionary<(int x, int y), NodeType>();

        public DijstraPathfinder(ICollection<NodeType> nodes) : base(nodes)
        {
            foreach (NodeType node in nodes)
            {
                nodesByCoordinate[node.GetCoordinate()] = node;
            }
        }

        protected override int Distance(NodeType A, NodeType B)
        {
            return 0;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            List<NodeType> neighbors = new List<NodeType>(directions.Length);
            (int x, int y) coordinate = node.GetCoordinate();

            foreach ((int x, int y) direction in directions)
            {
                if (nodesByCoordinate.TryGetValue((coordinate.x + direction.x, coordinate.y + direction.y),
                        out NodeType neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        protected override bool IsBlocked(NodeType node)
        {
            return node.IsBlocked();
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            (int x, int y) a = A.GetCoordinate();
            (int x, int y) b = B.GetCoordinate();
            (int width, int height) dimensions = A.GetDimensions();
            return (Math.Abs(b.x - a.x) * dimensions.width + Math.Abs(b.y - a.y) * dimensions.height) * B.GetCost();
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            return A.GetCoordinate() == B.GetCoordinate();
        }
    }
}
