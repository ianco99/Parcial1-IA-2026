using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Map.Events
{
	public struct TileCreatedEvent : IEvent
	{
		public int tileId;
		public int xCoord;
		public int yCoord;
		public int teamId;

		public void Assign(params object[] parameters)
		{
			tileId = (int)parameters[0];
			xCoord = (int)parameters[1];
			yCoord = (int)parameters[2];
			teamId = (int)parameters[3];
		}

		public void Reset()
		{
			tileId = 0;
			xCoord = -1;
			yCoord = -1;
			teamId = 0;
		}
	}
}