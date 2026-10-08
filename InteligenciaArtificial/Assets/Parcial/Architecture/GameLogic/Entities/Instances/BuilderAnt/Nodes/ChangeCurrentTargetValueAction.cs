using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class ChangeCurrentTargetValueAction : ActionNode
	{
		private int newValue;

		public ChangeCurrentTargetValueAction(string targetKey, int newValue)
		{
			this.newValue = newValue;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.SetData(BuilderAnt.TARGET_KEY, newValue);
			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] ChangeCurrentTargetValueAction: target set to {(BuilderAnt.Targets)newValue} -> Success");
			return NodeState.Success;
		}
	}

	public class CollectFoodAction : ActionNode
	{
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			uint entityID = (uint)enId;
			
			
			foreach (BuilderAnt ant in EntityRegistry.BuilderAnts)
			{
				if (ant.ID == entityID)
				{
					(Point, uint)foodData = Scene.GetNeighboringFood(ant.Coordinate.Origin);
					if (foodData.Item1 != default(Point))
					{
						BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CollectFoodAction: ant {entityID} requests food {foodData.Item2} -> Success");
						EventBus.Raise<RequestCollectFoodEvent>(ant.ID, ant.teamID, foodData.Item2);
						return NodeState.Success;
					}
					else
					{
						BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CollectFoodAction: ant {entityID} has no neighboring food -> Failure");
						return NodeState.Failure;
					}
				}
			}

			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] CollectFoodAction: ant {entityID} -> Failure");
			return NodeState.Failure;
		}
	}
}