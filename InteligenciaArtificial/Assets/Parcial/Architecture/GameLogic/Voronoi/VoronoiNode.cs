using ianco99.ToolBox.Services;
using Parcial.Architecture.Config;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public readonly struct VoronoiNode
	{
		public readonly uint id;
		public readonly Point tile;

        private MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();

		public VoronoiNode(uint id, Point tile)
		{
			this.id = id;
			this.tile = tile;
		}

		internal double X => tile.x + MapConfigReader.TileWidth / 2.0d;
		internal double Y => tile.y + MapConfigReader.TileHeight / 2.0d;
	}
}
