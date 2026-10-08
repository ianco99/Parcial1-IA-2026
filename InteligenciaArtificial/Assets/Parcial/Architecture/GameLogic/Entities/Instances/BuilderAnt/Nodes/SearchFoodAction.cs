using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class SearchFoodAction : ActionNode
	{
		public override NodeState Run(Blackboard blackboard)
		{
			BuilderAnt.Log(blackboard, "[BuilderAnt BT] SearchFoodAction: not implemented");
			throw new System.NotImplementedException();
		}
	}
}