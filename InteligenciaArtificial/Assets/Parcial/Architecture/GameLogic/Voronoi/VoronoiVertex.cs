using System.Collections.Generic;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class VoronoiVertex
	{
		public readonly float x;
		public readonly float y;
		public readonly bool IsOnMapBorder;

		internal readonly List<VoronoiArea> areas;

		public IReadOnlyList<VoronoiArea> Areas => areas;

		internal VoronoiVertex(double x, double y, bool isOnMapBorder)
		{
			this.x = (float)x;
			this.y = (float)y;
			IsOnMapBorder = isOnMapBorder;
			areas = new List<VoronoiArea>(3);
		}
	}
}
