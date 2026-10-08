using System.Threading.Tasks;

namespace KuznickiAgents.StatePattern.BehaviourTree
{
    public sealed class ParallelNode : BTNode
    {
        private readonly bool parallel;
        private readonly NodeState[] frameResults;

        public ParallelNode(bool parallel, params BTNode[] nodes) : base(nodes)
        {
            this.parallel = parallel;
            frameResults = new NodeState[nodes.Length];
        }

        public override NodeState Run(Blackboard blackboard)
        {
            if (parallel)
            {
                Parallel.For(0, Childrens.Count, blackboard.ParallelOptions, (i) =>
                {
                    frameResults[i] = Childrens[i].Run(blackboard);
                });
            }
            else
            {
                for (int i = 0; i < Childrens.Count; i++)
                {
                    frameResults[i] = Childrens[i].Run(blackboard);
                }
            }

            if (frameResults.Length == 1)
                return frameResults[0];

            foreach (NodeState nodeState in frameResults)
            {
                if (nodeState == NodeState.Failure)
                    return NodeState.Failure;
            }

            foreach (NodeState nodeState in frameResults)
            {
                if (nodeState == NodeState.Running)
                    return NodeState.Running;
            }

            return NodeState.Success;
        }
    }
}

