namespace KuznickiAgents.StatePattern.BehaviourTree
{
    public sealed class ForceFailDecoratorNode : DecoratorNode
    {
        public ForceFailDecoratorNode(BTNode child) : base(child)
        {
        }

        public override NodeState Run(Blackboard blackboard)
        {
            NodeState childState = Childrens[0].Run(blackboard);
            if (childState == NodeState.Running)
                return NodeState.Running;
            return NodeState.Failure;
        }
    }
}
