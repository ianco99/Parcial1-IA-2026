using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class CreatePathAction : ActionNode
	{
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private string destinationEntityKey;

		public CreatePathAction(string destinationEntityKey)
		{
			this.destinationEntityKey = destinationEntityKey;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			blackboard.TryGetData(destinationEntityKey, out object destId);

			uint entityID = (uint)enId;
			uint destinationID = (uint)destId;

			LivingEntity.LivingEntity entity = (LivingEntity.LivingEntity)EntityRegistry[entityID];

			if (!EntityRegistry.TryGet(destinationID, out Entity destinationEntity))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CreatePathAction: destination entity {destinationID} no longer exists -> Failure");
				return NodeState.Failure;
			}

			Point destination = destinationEntity.Coordinate.Origin;

			if (!entity.FindPath(destination))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CreatePathAction: ant {entityID} found no path to entity {destinationID} at ({destination.x}, {destination.y}) -> Failure");
				return NodeState.Failure;
			}

			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CreatePathAction: ant {entityID} has a path to entity {destinationID} at ({destination.x}, {destination.y}) -> Success");
			return NodeState.Success;
		}
	}
}
