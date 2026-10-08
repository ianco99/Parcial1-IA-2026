using System.Collections.Generic;

namespace KuznickiAgents.StatePattern.BehaviourTree
{
    public abstract class BTNode
    {
        protected List<BTNode> Childrens;

        protected BTNode(params BTNode[] nodes)
        {
            this.Childrens = new List<BTNode>(nodes);
        }

        public abstract NodeState Run(Blackboard blackboard);
    }
}

