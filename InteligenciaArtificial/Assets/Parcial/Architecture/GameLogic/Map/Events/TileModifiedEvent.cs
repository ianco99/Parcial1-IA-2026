using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Map.Events
{
	public struct TileModifiedEvent : IEvent
	{
		public int newTileId;
		public int xCoord;
		public int yCoord;
		public int newTeamId;

		public void Assign(params object[] parameters)
		{
			newTileId = (int)parameters[0];
			xCoord = (int)parameters[1];
			yCoord = (int)parameters[2];
			newTeamId = (int)parameters[3];
		}

		public void Reset()
		{
			newTileId = 0;
			xCoord = -1;
			yCoord = -1;
			newTeamId = 0;
		}
	}
}