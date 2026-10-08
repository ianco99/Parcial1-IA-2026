using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
	public Transform target;
	public int boidCount = 50;
	public Boid boidPrefab;
	private List<Boid> boids = new List<Boid>();

	private void Start()
	{
		for (int i = 0; i < boidCount; i++)
		{
			GameObject boidGO = Instantiate(boidPrefab.gameObject, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);
			Boid boid = boidGO.GetComponent<Boid>();
			boid.Init(Alignment, Cohesion, Separation, Direction);
			boids.Add(boid);
		}
	}

	public Vector3 Alignment(Boid boid)
	{
		var neighbors = GetBoidsInsideRadius(boid);
		if (neighbors.Count == 0)
			return Vector3.zero;

		Vector3 avg = Vector3.zero;
		foreach (Boid b in neighbors)
		{
			avg += (Vector3)b.transform.up;
		}

		return (avg / neighbors.Count).normalized;
	}

	public Vector3 Cohesion(Boid boid)
	{
		var neighbors = GetBoidsInsideRadius(boid);
		if (neighbors.Count == 0)
			return Vector3.zero;

		Vector3 avg = Vector3.zero;
		foreach (Boid b in neighbors)
		{
			avg += (Vector3)b.transform.position;
		}

		avg /= neighbors.Count;
		return (avg - (Vector3)boid.transform.position).normalized;
	}

	public Vector3 Separation(Boid boid)
	{
		var neighbors = GetBoidsInsideRadius(boid);
		if (neighbors.Count == 0)
			return Vector3.zero;

		Vector3 avg = Vector3.zero;
		foreach (Boid b in neighbors)
		{
			avg += ((Vector3)boid.transform.position - (Vector3)b.transform.position);
		}

		return (avg / neighbors.Count).normalized;
	}

	public Vector3 Direction(Boid boid)
	{
		return ((Vector3)target.position - (Vector3)boid.transform.position).normalized;
	}

	public List<Boid> GetBoidsInsideRadius(Boid boid)
	{
		List<Boid> insideRadiusBoids = new List<Boid>();

		foreach (Boid b in boids)
		{
			if (Vector3.Distance(boid.transform.position, b.transform.position) < boid.detectionRadious)
			{
				insideRadiusBoids.Add(b);
			}
		}

		return insideRadiusBoids;
	}
}