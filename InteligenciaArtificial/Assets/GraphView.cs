using KuznickiAgents.Pathfinding;
using UnityEngine;

public class GraphView : MonoBehaviour
{
	public Vector2IntGraph Graph;

	void Awake()
	{
		Graph = new Vector2IntGraph(10, 10);

		Graph[4, 0].SetIsBlocked(true);
		Graph[4, 1].SetIsBlocked(true);
		Graph[4, 2].SetIsBlocked(true);
		Graph[4, 4].SetIsBlocked(true);
		Graph[4, 6].SetIsBlocked(true);
		Graph[4, 7].SetIsBlocked(true);
		Graph[4, 8].SetIsBlocked(true);
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
			return;
		foreach (PathfindingNode node in Graph.nodes)

		{
			if (node.IsBlocked())
				Gizmos.color = Color.red;
			else
				Gizmos.color = Color.green;

			Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.1f);
		}
	}
}