namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public abstract class ConditionalNode : BTNode
	{
		protected ConditionalNode(BTNode child) : base(child)
		{
		}

		protected abstract bool Condition();

		public override NodeState Run(Blackboard blackboard)
		{
			if (Condition())
				return Childrens[0].Run(blackboard);
			return NodeState.Failure;
		}
	}
}

