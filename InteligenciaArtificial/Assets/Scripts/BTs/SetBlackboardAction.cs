using KuznickiAgents.StatePattern.BehaviourTree;
using UnityEngine;

public class SetBlackboardDataAction : ActionNode
{
	private readonly Transform[] mines;

	public SetBlackboardDataAction(Transform[] mines)
	{
		this.mines = mines;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		if (!blackboard.AddData("Mines", mines))
			return NodeState.Failure;

		if (!blackboard.AddData("AttackRange", 1.0f))
			return NodeState.Failure;

		if (!blackboard.AddData("GoldNeeded", 50.0f))
			return NodeState.Failure;

		return NodeState.Success;
	}
}