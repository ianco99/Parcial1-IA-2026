using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public readonly struct VoronoiNode
	{
		public readonly uint id;
		public readonly Point tile;

		public VoronoiNode(uint id, Point tile)
		{
			this.id = id;
			this.tile = tile;
		}

		internal double X => tile.x + 0.5;
		internal double Y => tile.y + 0.5;
	}
}
