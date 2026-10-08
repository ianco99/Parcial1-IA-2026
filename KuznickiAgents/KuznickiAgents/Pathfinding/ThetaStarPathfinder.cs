using System;
using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
    public class ThetaStarPathfinder<TNodeType> : Pathfinder<TNodeType> where TNodeType : IPathfindingNode
    {
        private static readonly (int x, int y)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };

        private readonly Dictionary<(int x, int y), TNodeType> nodesByCoordinate =
            new Dictionary<(int x, int y), TNodeType>();

        public ThetaStarPathfinder(ICollection<TNodeType> nodes) : base(nodes)
        {
            foreach (TNodeType node in nodes)
            {
                nodesByCoordinate[node.GetCoordinate()] = node;
            }
        }

        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, ICollection<TNodeType> graph)
        {
            List<TNodeType> aStarPath = base.FindPath(startNode, destinationNode, graph);

            if (aStarPath == null)
                return null;

            List<TNodeType> fullPath = new List<TNodeType>(aStarPath.Count + 1);
            fullPath.Add(startNode);
            fullPath.AddRange(aStarPath);

            List<TNodeType> path = new List<TNodeType>(aStarPath.Count);

            int currentIndex = 0;

            while (currentIndex < fullPath.Count - 1)
            {
                bool foundShortcut = false;

                for (int targetIndex = fullPath.Count - 1; targetIndex > currentIndex + 1; targetIndex--)
                {
                    if (TryGetShortcut(fullPath, currentIndex, targetIndex, out List<TNodeType> line))
                    {
                        path.AddRange(line);
                        currentIndex = targetIndex;
                        foundShortcut = true;
                        break;
                    }
                }

                if (!foundShortcut)
                {
                    currentIndex++;
                    path.Add(fullPath[currentIndex]);
                }
            }

            return path;
        }

        private bool TryGetShortcut(List<TNodeType> fullPath, int fromIndex, int toIndex, out List<TNodeType> line)
        {
            line = null;

            for (int i = fromIndex; i <= toIndex; i++)
            {
                if (!CanOptimizeThrough(fullPath[i]))
                    return false;
            }

            (int x, int y) from = fullPath[fromIndex].GetCoordinate();
            (int x, int y) to = fullPath[toIndex].GetCoordinate();

            int deltaX = Math.Abs(to.x - from.x);
            int deltaY = Math.Abs(to.y - from.y);
            int stepX = from.x < to.x ? 1 : -1;
            int stepY = from.y < to.y ? 1 : -1;
            int error = deltaX - deltaY;

            List<TNodeType> lineNodes = new List<TNodeType>(Math.Max(deltaX, deltaY));
            (int x, int y) current = from;

            while (current != to)
            {
                int doubleError = 2 * error;

                if (doubleError > -deltaY)
                {
                    error -= deltaY;
                    current.x += stepX;
                }

                if (doubleError < deltaX)
                {
                    error += deltaX;
                    current.y += stepY;
                }

                if (!nodesByCoordinate.TryGetValue(current, out TNodeType node) || !CanOptimizeThrough(node))
                    return false;

                lineNodes.Add(node);
            }

            line = lineNodes;
            return true;
        }

        protected virtual bool CanOptimizeThrough(TNodeType node)
        {
            return !IsBlocked(node);
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
