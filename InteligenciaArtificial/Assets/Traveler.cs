using System.Collections;
using System.Collections.Generic;
using KuznickiAgents.Pathfinding;
using UnityEngine;

public class Traveler : MonoBehaviour
{
    public GraphView graphView;
    
    [SerializeField] private PathfindingAlgorithm algorithm = PathfindingAlgorithm.AStar;

    private Pathfinder<PathfindingNode> pathfinder;

    private List<PathfindingNode> path;
        
    private PathfindingNode startPathfindingNode; 
    private PathfindingNode destinationPathfindingNode;

    void Start()
    {
        List<PathfindingNode> nodes = graphView.Graph.nodes;

        switch (algorithm)
        {
            case PathfindingAlgorithm.Dijkstra:
                pathfinder = new DijstraPathfinder<PathfindingNode>(nodes);
                break;
            case PathfindingAlgorithm.BreadthFirst:
                pathfinder = new BreadthFirstPathfinder<PathfindingNode>(nodes);
                break;
            case PathfindingAlgorithm.DepthFirst:
                pathfinder = new DepthFirstPathfinder<PathfindingNode>(nodes);
                break;
            case PathfindingAlgorithm.ThetaStar:
                pathfinder = new ThetaStarPathfinder<PathfindingNode>(nodes);
                break;
            default:
                pathfinder = new AStarPathfinder<PathfindingNode>(nodes);
                break;
        }

        startPathfindingNode = nodes[Random.Range(0, 10)];
        do
        {
            destinationPathfindingNode = nodes[Random.Range(40, nodes.Count)];
        } while (destinationPathfindingNode.IsBlocked());

        path = pathfinder.FindPath(startPathfindingNode, destinationPathfindingNode, nodes);

        if (path == null)
        {
            Debug.LogWarning("No path found from " + startPathfindingNode.GetCoordinate() + " to " + destinationPathfindingNode.GetCoordinate());
            return;
        }

        StartCoroutine(Move(path));
    }

    public IEnumerator Move(List<PathfindingNode> path) 
    {
        foreach (PathfindingNode node in path)
        {
            transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (path == null)
            return;

        foreach (PathfindingNode node in path)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.3f);
        }
            
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(startPathfindingNode.GetCoordinate().x, startPathfindingNode.GetCoordinate().y), 0.3f);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(new Vector3(destinationPathfindingNode.GetCoordinate().x, destinationPathfindingNode.GetCoordinate().y), 0.3f);
    }
}