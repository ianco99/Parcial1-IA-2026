using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class LeaveFoodAction : ActionNode
	{
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			blackboard.TryGetData(BuilderAnt.QUEEN_ID_KEY, out object qnId);

			uint entityID = (uint)enId;
			uint queenID = (uint)qnId;

			BuilderAnt ant = (BuilderAnt)EntityRegistry[entityID];

			EventBus.Raise<RequestLeaveFoodEvent>(ant.ID, ant.teamID, queenID);

			if (ant.HasFood)
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] LeaveFoodAction: ant {entityID} found no room next to queen {queenID} -> Running");
				return NodeState.Running;
			}

			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] LeaveFoodAction: ant {entityID} left food next to queen {queenID} -> Success");
			return NodeState.Success;
		}
	}
}
