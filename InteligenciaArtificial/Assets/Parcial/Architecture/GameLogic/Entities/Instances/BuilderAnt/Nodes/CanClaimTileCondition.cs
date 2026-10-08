using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.ClaimTile;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class CanClaimTileCondition : ConditionalNode
	{
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		private int teamID;
		private bool logsEnabled;

		public CanClaimTileCondition(BTNode child, int teamID) : base(child)
		{
			this.teamID = teamID;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			logsEnabled = BuilderAnt.LogsEnabled(blackboard);
			return base.Run(blackboard);
		}

		protected override bool Condition()
		{
			ClaimTileControllerArchitecture claimController =
				Scene.GetSpawnTeamControllerArchitecture(teamID).ClaimTileControllerArchitecture;

			if (!claimController.HasFoodToClaim)
			{
				if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] CanClaimTileCondition: team {teamID} has less than {ClaimTileControllerArchitecture.CLAIM_FOOD_REQUIRED} food stored -> False");
				return false;
			}

			if (claimController.IsOnCooldown)
			{
				if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] CanClaimTileCondition: team {teamID} is on claim cooldown -> False");
				return false;
			}

			if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] CanClaimTileCondition: team {teamID} can claim a tile -> True");
			return true;
		}
	}
}
