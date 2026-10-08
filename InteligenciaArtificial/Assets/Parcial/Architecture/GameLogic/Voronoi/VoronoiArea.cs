using System.Collections.Generic;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class VoronoiArea
	{
		public readonly VoronoiNode Node;

		internal List<VoronoiVertex> vertices;
		internal List<uint> edgeTags;

		// The same vertices as plain points, which is what the shared polygon helpers take.
		internal (double x, double y)[] polygon;
		internal readonly List<VoronoiArea> neighbors;
		private readonly List<VoronoiSegment> segments;

		public IReadOnlyList<VoronoiVertex> Vertices => vertices;
		public IReadOnlyList<VoronoiSegment> Segments => segments;
		public IReadOnlyList<VoronoiArea> Neighbors => neighbors;

		internal VoronoiArea(VoronoiNode node)
		{
			Node = node;
			vertices = new List<VoronoiVertex>();
			edgeTags = new List<uint>();
			polygon = new (double x, double y)[0];
			neighbors = new List<VoronoiArea>();
			segments = new List<VoronoiSegment>();
		}

		internal void SetGeometry(List<VoronoiVertex> newVertices, List<uint> newEdgeTags)
		{
			vertices = newVertices;
			edgeTags = newEdgeTags;

			polygon = new (double x, double y)[vertices.Count];
			segments.Clear();

			for (int i = 0; i < vertices.Count; i++)
			{
				polygon[i] = (vertices[i].x, vertices[i].y);
				segments.Add(new VoronoiSegment(vertices[i], vertices[(i + 1) % vertices.Count], edgeTags[i]));
			}
		}

		internal List<VoronoiDiagram.RawPoint> GetRawVertices()
		{
			List<VoronoiDiagram.RawPoint> polygon = new List<VoronoiDiagram.RawPoint>();

			for (int i = 0; i < vertices.Count; i++)
			{
				polygon.Add(new VoronoiDiagram.RawPoint(vertices[i].x, vertices[i].y, edgeTags[i]));
			}

			return polygon;
		}
	}
}
