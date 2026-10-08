namespace KuznickiAgents.StatePattern.BehaviourTree
{
    public sealed class ForceSuccessDecoratorNode : DecoratorNode
    {
        public ForceSuccessDecoratorNode(BTNode child) : base(child)
        {
        }

        public override NodeState Run(Blackboard blackboard)
        {
            NodeState childState = Childrens[0].Run(blackboard);
            if (childState == NodeState.Running)
                return NodeState.Running;
            return NodeState.Success;
        }
    }
}
