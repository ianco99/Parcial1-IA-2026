namespace Parcial.Architecture.GameLogic.Voronoi
{
	public readonly struct VoronoiSegment
	{
		public readonly VoronoiVertex a;
		public readonly VoronoiVertex b;

		public readonly uint neighborId;

		public bool IsMapBorder => neighborId == VoronoiDiagram.MAP_BORDER_ID;

		internal VoronoiSegment(VoronoiVertex a, VoronoiVertex b, uint neighborId)
		{
			this.a = a;
			this.b = b;
			this.neighborId = neighborId;
		}
	}
}
