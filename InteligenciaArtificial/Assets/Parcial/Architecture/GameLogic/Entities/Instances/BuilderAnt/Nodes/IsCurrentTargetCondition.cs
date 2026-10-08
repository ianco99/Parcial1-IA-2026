using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class IsCurrentTargetCondition : ConditionalNode
	{
		private int target;
		private int currentTarget;

		public IsCurrentTargetCondition(BTNode child, int target) : base(child)
		{
			this.target = target;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			if (!blackboard.TryGetData(BuilderAnt.TARGET_KEY, out object data))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] IsCurrentTargetCondition({(BuilderAnt.Targets)target}): no target in blackboard -> Failure");
				return NodeState.Failure;
			}

			currentTarget = (int)data;
			BuilderAnt.Log(blackboard, $"[BuilderAnt BT] IsCurrentTargetCondition({(BuilderAnt.Targets)target}): current={(BuilderAnt.Targets)currentTarget} -> {currentTarget == target}");
			return base.Run(blackboard);
		}

		protected override bool Condition()
		{
			return currentTarget == target;
		}
	}
}