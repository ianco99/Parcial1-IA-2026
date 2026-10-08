using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Voronoi.Events
{
	public struct WeightedVoronoiRecalculatedEvent : IEvent
	{
		public int teamId;
		public WeightedVoronoiDiagram diagram;

		public void Assign(params object[] parameters)
		{
			teamId = (int)parameters[0];
			diagram = (WeightedVoronoiDiagram)parameters[1];
		}

		public void Reset()
		{
			teamId = 0;
			diagram = null;
		}
	}
}
