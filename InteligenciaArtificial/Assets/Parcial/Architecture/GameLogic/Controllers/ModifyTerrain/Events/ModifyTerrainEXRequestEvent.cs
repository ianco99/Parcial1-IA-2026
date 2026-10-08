using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events
{
	public struct ModifyTerrainEXRequestEvent : IEvent
	{
		public Point origin;
		public Point end;
		public int[] tileExceptions;
		public string newTileId;
		public int terrainTeamId;

		public void Assign(params object[] parameters)
		{
			origin = (Point)parameters[0];
			end = (Point)parameters[1];
			tileExceptions = (int[])parameters[2];
			newTileId = (string)parameters[3];
			terrainTeamId = (int)parameters[4];
		}

		public void Reset()
		{
			origin = default(Point);
			end = default(Point);
			tileExceptions = default(int[]);
			newTileId = default(string);
			terrainTeamId = default(int);
		}
	}
}