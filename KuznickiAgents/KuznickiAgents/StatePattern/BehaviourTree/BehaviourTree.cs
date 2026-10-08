namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public class BehaviourTree
	{
		private RootNode root;
		private Blackboard blackboard;

		public BehaviourTree(RootNode root)
		{
			this.root = root;
			this.blackboard = new Blackboard();
		}

		public void Tick() 
		{
			root.Tick(blackboard);
		}
	}
}