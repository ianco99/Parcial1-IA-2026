using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class SetTargetToClaimTileAction : ActionNode
	{
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			uint entityID = (uint)enId;

			LivingEntity.LivingEntity entity = (LivingEntity.LivingEntity)EntityRegistry[entityID];

			if (!Scene.TryGetClaimableTile(entity.teamID, entity.Coordinate.Origin, out Point tile))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] SetTargetToClaimTileAction: no default tile next to the anthill of team {entity.teamID} -> Failure");
				return NodeState.Failure;
			}

			if (!entity.FindPath(tile))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] SetTargetToClaimTileAction: ant {entityID} found no path to tile ({tile.x}, {tile.y}) -> Failure");
				return NodeState.Failure;
			}

			blackboard.SetData(BuilderAnt.CLAIM_TILE_KEY, tile);
			blackboard.SetData(BuilderAnt.TARGET_KEY, (int)BuilderAnt.Targets.Tile);
			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] SetTargetToClaimTileAction: ant {entityID} target set to Tile ({tile.x}, {tile.y}) -> Success");
			return NodeState.Success;
		}
	}
}
