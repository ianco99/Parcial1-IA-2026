namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public sealed class SequenceNode : BTNode
	{
		public SequenceNode(params BTNode[] childs) : base(childs)
		{
		}

		public override NodeState Run(Blackboard blackboard)
		{
			for (int i = 0; i < Childrens.Count; i++)
			{
				NodeState childResult = Childrens[i].Run(blackboard);
				if (childResult == NodeState.Running)
					return NodeState.Running;
				if (childResult == NodeState.Failure)
					return NodeState.Failure;
			}
			return NodeState.Success;
		}
	}
}

