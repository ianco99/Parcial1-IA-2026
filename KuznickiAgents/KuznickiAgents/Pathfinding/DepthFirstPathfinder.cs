using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
    public class DepthFirstPathfinder<TNodeType> : Pathfinder<TNodeType> where TNodeType : IPathfindingNode
    {
        private static readonly (int x, int y)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };

        private readonly Dictionary<(int x, int y), TNodeType> nodesByCoordinate =
            new Dictionary<(int x, int y), TNodeType>();

        public DepthFirstPathfinder(ICollection<TNodeType> nodes) : base(nodes)
        {
            foreach (TNodeType node in nodes)
            {
                nodesByCoordinate[node.GetCoordinate()] = node;
            }
        }

        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, ICollection<TNodeType> graph)
        {
            Dictionary<(int x, int y), TNodeType> parents = new Dictionary<(int x, int y), TNodeType>(graph.Count);

            Stack<(TNodeType Node, TNodeType Parent)> openStack = new Stack<(TNodeType Node, TNodeType Parent)>();

            openStack.Push((startNode, startNode));

            while (openStack.Count > 0)
            {
                (TNodeType currentNode, TNodeType parent) = openStack.Pop();

                if (parents.ContainsKey(currentNode.GetCoordinate()))
                {
                    continue;
                }

                parents.Add(currentNode.GetCoordinate(), parent);

                if (NodesEquals(currentNode, destinationNode))
                {
                    return GeneratePath();
                }

                foreach (TNodeType neighbor in GetNeighbors(currentNode))
                {
                    if (IsBlocked(neighbor) ||
                        parents.ContainsKey(neighbor.GetCoordinate()))
                    {
                        continue;
                    }

                    openStack.Push((neighbor, currentNode));
                }
            }

            return null;

            List<TNodeType> GeneratePath()
            {
                List<TNodeType> path = new List<TNodeType>();
                TNodeType currentNode = destinationNode;

                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);
                    currentNode = parents[currentNode.GetCoordinate()];
                }

                path.Reverse();
                return path;
            }
        }

        protected override int Distance(TNodeType A, TNodeType B)
        {
            return 0;
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
            return 1;
        }

        protected override bool NodesEquals(TNodeType A, TNodeType B)
        {
            return A.GetCoordinate() == B.GetCoordinate();
        }
    }
}
