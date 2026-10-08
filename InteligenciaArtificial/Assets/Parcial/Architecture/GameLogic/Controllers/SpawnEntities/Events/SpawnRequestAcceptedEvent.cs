using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events
{
	public struct SpawnRequestAcceptedEvent<TEntityType> : IEvent
	{
		public string BlueprintToSpawn;
		public Coordinate CoordinateToSpawn;
		public string BlueprintFile;
		public int teamID;

		public void Assign(params object[] parameters)
		{
			BlueprintToSpawn = (string)parameters[0];
			CoordinateToSpawn = (Coordinate)parameters[1];
			BlueprintFile = (string)parameters[2];
			teamID = (int)parameters[3];
		}

		public void Reset()
		{
			BlueprintToSpawn = string.Empty;
			CoordinateToSpawn = default;
			BlueprintFile = null;
			teamID = 0;
		}
	}
}