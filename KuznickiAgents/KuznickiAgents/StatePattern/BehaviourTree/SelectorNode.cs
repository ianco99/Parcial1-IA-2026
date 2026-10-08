namespace KuznickiAgents.StatePattern.BehaviourTree
{
    public sealed class SelectorNode : BTNode
    {
        public SelectorNode(params BTNode[] childs) : base(childs)
        {
        }

        public override NodeState Run(Blackboard blackboard)
        {
            for (int i = 0; i < Childrens.Count; i++)
            {
                NodeState childResult = Childrens[i].Run(blackboard);
                if (childResult == NodeState.Success)
                    return NodeState.Success;
                if (childResult == NodeState.Running)
                    return NodeState.Running;
            }
            return NodeState.Failure;
        }
    }
}

