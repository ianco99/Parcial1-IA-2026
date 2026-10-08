namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public abstract class DecoratorNode : BTNode
	{
		protected DecoratorNode(BTNode child) : base(child)
		{
		}
	}
}

