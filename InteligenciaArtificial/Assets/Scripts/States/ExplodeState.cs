using KuznickiAgents.StatePattern;
using UnityEngine;

public sealed class ExplodeState : State
{
	public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
	{
		BehaviourActions behaviourActions = new BehaviourActions();
		behaviourActions.AddMultiThreadableBehaviour(0, () => { Debug.Log("Boom"); });
		return behaviourActions;
	}

	public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
	{
		return default;
	}

	public override BehaviourActions GetTickBehaviours(params object[] parameters)
	{
		BehaviourActions behaviourActions = new BehaviourActions();
		behaviourActions.AddMultiThreadableBehaviour(0, () => { Debug.Log("F"); });
		return behaviourActions;
	}
}