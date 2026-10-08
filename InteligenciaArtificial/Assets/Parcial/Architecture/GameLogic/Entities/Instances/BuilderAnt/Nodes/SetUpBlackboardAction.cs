using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class SetUpBlackboardAction : ActionNode
	{
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private uint entityID;
		private int teamID;
		private int entitySpeed;
		private uint foodID;
		private int target;

		private bool logsEnabled = false;

		private bool isSetUp = false;

		public SetUpBlackboardAction(uint entityID, int teamID, int entitySpeed, uint foodID, int target)
		{
			this.entityID = entityID;
			this.teamID = teamID;
			this.entitySpeed = entitySpeed;
			this.foodID = foodID;
			this.target = target;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			if (isSetUp)
				return NodeState.Success;

			uint queenID = Entity.UNASSIGNED_ENTITY_ID;

			foreach (QueenAnt queen in EntityRegistry.FilterEntities<QueenAnt>())
			{
				if (queen.teamID == teamID)
				{
					queenID = queen.ID;
					break;
				}
			}

			if (queenID == Entity.UNASSIGNED_ENTITY_ID)
			{
				if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] SetUpBlackboardAction: ant {entityID} found no queen for team {teamID} -> Failure");
				return NodeState.Failure;
			}

			blackboard.AddData(BuilderAnt.ENTITY_ID_KEY, entityID);
			blackboard.AddData(BuilderAnt.ENTITY_SPEED_KEY, entitySpeed);
			blackboard.AddData(BuilderAnt.FOOD_ID_KEY, foodID);
			blackboard.AddData(BuilderAnt.QUEEN_ID_KEY, queenID);
			blackboard.AddData(BuilderAnt.TARGET_KEY, target);
			blackboard.AddData(BuilderAnt.CLAIM_TILE_KEY, default(Point));
			blackboard.AddData(BuilderAnt.LOGS_ENABLED_KEY, logsEnabled);

			if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] SetUpBlackboardAction: ant {entityID} seeded speed={entitySpeed}, foodID={foodID}, queenID={queenID}, target={(BuilderAnt.Targets)target}");

			isSetUp = true;
			return NodeState.Success;
		}
	}
}
