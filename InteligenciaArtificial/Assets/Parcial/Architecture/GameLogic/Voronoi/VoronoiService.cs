using ianco99.ToolBox.Services;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class VoronoiService : IService
	{
		public bool IsPersistance => false;

		public VoronoiDiagram CreateDiagram(int width, int height)
		{
			return new VoronoiDiagram(width, height);
		}
	}
}
