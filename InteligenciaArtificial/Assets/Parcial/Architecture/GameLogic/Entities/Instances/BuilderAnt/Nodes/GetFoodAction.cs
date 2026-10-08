using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class GetFoodAction : ActionNode
	{
		public override NodeState Run(Blackboard blackboard)
		{
			BuilderAnt.Log(blackboard, "[BuilderAnt BT] GetFoodAction -> Success");
			return NodeState.Success;
		}
	}
}