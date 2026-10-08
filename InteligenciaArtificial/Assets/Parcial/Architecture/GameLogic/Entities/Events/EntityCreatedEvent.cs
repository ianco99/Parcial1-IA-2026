using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct EntityCreatedEvent<TEntityType> : IEvent
		where TEntityType : Entity
	{
		public string BlueprintId;
		public string BlueprintTable;
		public uint EntityCreatedId;
		public Coordinate Coordinate;
		public int entityTeamID;

		public void Assign(params object[] parameters)
		{
			BlueprintId = (string)parameters[0];
			BlueprintTable = (string)parameters[1];
			EntityCreatedId = (uint)parameters[2];
			Coordinate = (Coordinate)parameters[3];
			entityTeamID = (int)parameters[4];
		}

		public void Reset()
		{
			BlueprintId = null;
			BlueprintTable = null;
			EntityCreatedId = Entity.UNASSIGNED_ENTITY_ID;
			Coordinate = default;
			entityTeamID = 0;
		}
	}
}