using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Voronoi.Events
{
	public struct FoodVoronoiRecalculatedEvent : IEvent
	{
		public VoronoiDiagram diagram;

		public void Assign(params object[] parameters)
		{
			diagram = (VoronoiDiagram)parameters[0];
		}

		public void Reset()
		{
			diagram = null;
		}
	}
}
