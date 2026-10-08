namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public class RootNode : BTNode
	{
		public RootNode(BTNode child) : base(child)
		{
		}

		public void Tick(Blackboard blackboard) 
		{
			Run(blackboard);
		}

		public override NodeState Run(Blackboard blackboard)
		{
			return Childrens[0].Run(blackboard);
		}
	}
}

