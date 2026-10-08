using ianco99.ToolBox.Events;
using KuznickiAgents.Pathfinding;
using Parcial.Architecture.GameLogic.Entities;

namespace Parcial.Architecture.GameLogic.Controllers.ChangePathfinding.Events
{
    public struct ChangePathfindingAlgorithmRequestAcceptedEvent : IEvent
    {
        public uint entityId;
        public PathfindingAlgorithm algorithm;

        public void Assign(params object[] parameters)
        {
            entityId = (uint)parameters[0];
            algorithm = (PathfindingAlgorithm)parameters[1];
        }

        public void Reset()
        {
            entityId = Entity.UNASSIGNED_ENTITY_ID;
            algorithm = PathfindingAlgorithm.AStar;
        }
    }
}
