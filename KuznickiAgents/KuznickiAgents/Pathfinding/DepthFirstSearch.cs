using System;
using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
	public class DepthFirstSearch<TNodeType> where TNodeType : IPathfindingNode
	{
		private readonly Stack<TNodeType> nodesVisited = new Stack<TNodeType>();
		private readonly Stack<TNodeType> nodesToVisit = new Stack<TNodeType>();

		public void FindPath(TNodeType source, TNodeType destination, ICollection<TNodeType> graph)
		{
			TNodeType currentPathfindingNode = source;

			nodesToVisit.Push(currentPathfindingNode);

			while (nodesToVisit.Count > 0)
			{
				TNodeType visiting = nodesToVisit.Pop();
				currentPathfindingNode = visiting;
				nodesVisited.Push(currentPathfindingNode);
				
				List<TNodeType> neighbors = GetNeighbors(visiting, graph);

			}
			
			RegeneratePath();
		}

		private List<TNodeType> GetNeighbors(TNodeType pathfindingNode, ICollection<TNodeType> graph)
		{
			List<TNodeType> neighbors = new List<TNodeType>();
			(int x, int y) coordinate = pathfindingNode.GetCoordinate();

			foreach (TNodeType candidate in graph)
			{
				(int x, int y) candidateCoordinate = candidate.GetCoordinate();

				int distance = Math.Abs(candidateCoordinate.x - coordinate.x) +
				               Math.Abs(candidateCoordinate.y - coordinate.y);

				if (distance == 1)
					neighbors.Add(candidate);
			}

			return neighbors;
		}
		
		private int GetAccumulativeCost(TNodeType source, TNodeType destination)
		{
			return 0;
		}

		private List<TNodeType> RegeneratePath()
		{
			List<TNodeType> path = new List<TNodeType>();

			for (int i = 0; i < nodesVisited.Count; i++)
			{
				path.Add(nodesVisited.Pop());
			}
			
			path.Reverse();

			return path;
		}
	}
}