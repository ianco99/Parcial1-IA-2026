using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.Anthill.Events
{
	public struct AnthillSpawnedEvent : IEvent
	{
		public Point origin;
		public Point end;
		public Point center;
		public int teamID;
		
		public void Assign(params object[] parameters)
		{
			origin = (Point)parameters[0];
			end = (Point)parameters[1];
			center = (Point)parameters[2];
			teamID = (int)parameters[3];
		}

		public void Reset()
		{
			origin = default(Point);
			end = default(Point);
			center = default(Point);
			teamID = 0;
		}
	}
}