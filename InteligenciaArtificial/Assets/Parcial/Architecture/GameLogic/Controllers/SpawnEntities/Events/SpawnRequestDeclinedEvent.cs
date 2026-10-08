using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events
{
	public struct SpawnRequestRejectedEvent<TEntityType> : IEvent
	{
		public string blueprintToSpawn;
		public Coordinate coordinateToSpawn;
		public int teamId;
		public string message;
		public void Assign(params object[] parameters)
		{
			blueprintToSpawn = (string)parameters[0];
			coordinateToSpawn = (Coordinate)parameters[1];
			teamId = (int)parameters[2];
			
			if (parameters.Length > 3)
				message = (string)parameters[3];
		}

		public void Reset()
		{
			blueprintToSpawn = string.Empty;
			coordinateToSpawn = default(Coordinate);
			message = default(string);
		}
	}

	public struct SpawnRequestRejectedEvent : IEvent
	{
		public string blueprintToSpawn;
		public Coordinate coordinateToSpawn;
		public string entityTypeName;
		public int teamId;
		public string message;

		public void Assign(params object[] parameters)
		{
			blueprintToSpawn = (string)parameters[0];
			coordinateToSpawn = (Coordinate)parameters[1];
			entityTypeName = (string)parameters[2];
			teamId = (int)parameters[3];
			
			if (parameters.Length > 3)
				message = (string)parameters[4];
		}

		public void Reset()
		{
			blueprintToSpawn = string.Empty;
			coordinateToSpawn = default(Coordinate);
			entityTypeName = default(string);
			teamId = default(int);
			message = default(string);
		}
	}
}