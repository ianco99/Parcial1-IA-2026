using System.Collections.Generic;
using KuznickiAgents.Pathfinding;

public class Vector2IntGraph
{
	public List<PathfindingNode> nodes = new List<PathfindingNode>();

	public Vector2IntGraph(int x, int y)
	{
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				PathfindingNode pathfindingNode = new PathfindingNode();
				pathfindingNode.SetCoordinate((i, j));
				nodes.Add(pathfindingNode);
			}
		}
	}

	public PathfindingNode this[int row, int col]
	{
		get
		{
			foreach (PathfindingNode node in nodes)
			{
				if (node.GetCoordinate() == (row, col))
					return node;
			}

			return null;
		}
	}
}