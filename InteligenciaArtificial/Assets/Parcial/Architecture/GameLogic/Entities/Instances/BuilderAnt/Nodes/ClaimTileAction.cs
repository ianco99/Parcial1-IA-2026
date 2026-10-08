using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class ClaimTileAction : ActionNode
	{
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			blackboard.TryGetData(BuilderAnt.CLAIM_TILE_KEY, out object claimTile);

			uint entityID = (uint)enId;
			Point tile = (Point)claimTile;

			Entity ant = EntityRegistry[entityID];

			EventBus.Raise<RequestClaimTileEvent>(ant.ID, ant.teamID, tile);

			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] ClaimTileAction: ant {entityID} requests tile ({tile.x}, {tile.y}) for team {ant.teamID} -> Success");
			return NodeState.Success;
		}
	}
}
