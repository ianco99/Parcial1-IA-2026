namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public sealed class InvertDecoratorNode : DecoratorNode
	{
		public InvertDecoratorNode(BTNode child) : base(child)
		{
		}

		public override NodeState Run(Blackboard blackboard)
		{
			NodeState childState = Childrens[0].Run(blackboard);
			switch (childState)
			{
				case NodeState.Success:
					return NodeState.Failure;
				case NodeState.Failure:
					return NodeState.Success;
			}
			return NodeState.Running;
		}
	}
}

