using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events
{
    public struct SpawnRequestEvent<TEntityType> : IEvent
    {
        public string blueprintToSpawn;
        public Coordinate coordinateToSpawn;
        public int teamId;

        public void Assign(params object[] parameters)
        {
            blueprintToSpawn = (string)parameters[0];
            coordinateToSpawn = (Coordinate)parameters[1];
            teamId = (int)parameters[2];
        }

        public void Reset()
        {
            blueprintToSpawn = string.Empty;
            coordinateToSpawn = default(Coordinate);
            teamId = 0;
        }
    }
}