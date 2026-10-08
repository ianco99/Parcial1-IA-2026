using System.Collections.Generic;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class WeightedVoronoiArea
	{
		public readonly VoronoiNode Node;

		internal VoronoiArea baseArea;
		internal (double x, double y)[] polygon;
		internal readonly List<WeightedVoronoiArea> neighbors;

		internal bool wasValid;

		public int Weight { get; internal set; }
		public int TileCount { get; internal set; }

		public double DeviationFromAverage { get; internal set; }

		public double AverageTileWeight => TileCount > 0 ? (double)Weight / TileCount : 0.0;

		public IReadOnlyList<(double x, double y)> Polygon => polygon;
		public IReadOnlyList<WeightedVoronoiArea> Neighbors => neighbors;

		internal WeightedVoronoiArea(VoronoiNode node)
		{
			Node = node;
			polygon = new (double x, double y)[0];
			neighbors = new List<WeightedVoronoiArea>();
		}

		public uint NeighborIdOf(int index)
		{
			return baseArea.edgeTags[index];
		}

		public bool Contains(double x, double y)
		{
			return VoronoiGeometry.Contains(polygon, x, y);
		}
	}
}
