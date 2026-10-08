using System;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class HasFoodCondition : ConditionalNode
	{
		private Func<bool> hasFood;
		private bool logsEnabled;

		public HasFoodCondition(BTNode child, Func<bool> hasFood) : base(child)
		{
			this.hasFood = hasFood;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			logsEnabled = BuilderAnt.LogsEnabled(blackboard);
			return base.Run(blackboard);
		}

		protected override bool Condition()
		{
			bool result = hasFood.Invoke();
			if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] HasFoodCondition -> {result}");
			return result;
		}
	}
}