using System.Linq;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class SetTargetToFoodAction : ActionNode
	{
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		public override NodeState Run(Blackboard blackboard)
		{
			blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object enId);
			Point antPosition = EntityRegistry[(uint)enId].Coordinate.Origin;

			uint closestFoodID = Scene.GetClosestFood(antPosition);

			if (closestFoodID == Entity.UNASSIGNED_ENTITY_ID)
			{
				BuilderAnt.Log(blackboard, "[BuilderAnt BT] SetTargetToFoodAction: no food spawned yet -> Failure");
				return NodeState.Failure;
			}

			if (!EntityRegistry.Foods.Any(food => food.ID == closestFoodID))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] SetTargetToFoodAction: closest food {closestFoodID} no longer exists -> Failure");
				return NodeState.Failure;
			}

			blackboard.SetData(BuilderAnt.FOOD_ID_KEY, closestFoodID);
			blackboard.SetData(BuilderAnt.TARGET_KEY, (int)BuilderAnt.Targets.Food);
			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] SetTargetToFoodAction: target set to Food {closestFoodID} -> Success");
			return NodeState.Success;
		}
	}
}
