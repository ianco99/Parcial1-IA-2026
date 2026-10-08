using System;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class WeightedVoronoiService : IService
	{
		public bool IsPersistance => false;

		public WeightedVoronoiDiagram CreateDiagram(int width, int height, Func<Point, int> tileWeight)
		{
			return new WeightedVoronoiDiagram(width, height, tileWeight);
		}
	}
}
