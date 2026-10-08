using KuznickiAgents.StatePattern;
using UnityEngine;

public sealed class PatrolState : State
{
	private Transform actualTarget;

	public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
	{
		return default;
	}

	public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
	{
		return default;
	}

	public override BehaviourActions GetTickBehaviours(params object[] parameters)
	{
		Transform wayPoint1 = parameters[0] as Transform;
		Transform wayPoint2 = parameters[1] as Transform;
		Transform agentTransform = parameters[2] as Transform;
		Transform targetTransform = parameters[3] as Transform;
		float speed = (float)parameters[4];
		float chaseDistance = (float)parameters[5];
		float deltaTime = (float)parameters[6];

		BehaviourActions behaviourActions = new BehaviourActions();

		behaviourActions.AddMainThreadableBehaviour(0, () =>
		{
			if (actualTarget == null)
			{
				actualTarget = wayPoint1;
			}

			if (Vector3.Distance(agentTransform.position, actualTarget.position) < 0.1f)
			{
				if (actualTarget == wayPoint1)
				{
					actualTarget = wayPoint2;
				}
				else
				{
					actualTarget = wayPoint1;
				}
			}
		});

		behaviourActions.AddMainThreadableBehaviour(1,
			() =>
			{
				agentTransform.position +=
					(actualTarget.position - agentTransform.position).normalized * speed * deltaTime;
			});

		behaviourActions.SetTransitionBehaviour(() =>
		{
			if (Vector3.Distance(agentTransform.position, targetTransform.position) <= chaseDistance)
			{
				OnTrigger?.Invoke(Agent.Flags.OnTargetNear.ToString());
			}
		});
		return behaviourActions;
	}
}

public sealed class ChaseState : State
{
	public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
	{
		return default;
	}

	public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
	{
		return default;
	}

	public override BehaviourActions GetTickBehaviours(params object[] parameters)
	{
		Transform agentTrnasform = parameters[0] as Transform;
		Transform targetTransform = parameters[1] as Transform;
		float speed = (float)parameters[2];
		float explodeDistance = (float)parameters[3];
		float lostDistance = (float)parameters[4];
		float deltaTime = (float)parameters[5];

		BehaviourActions behaviourActions = new BehaviourActions();

		behaviourActions.AddMainThreadableBehaviour(0,
			() =>
			{
				agentTrnasform.position +=
					(targetTransform.position - agentTrnasform.position).normalized * speed * deltaTime;
			});

		behaviourActions.SetTransitionBehaviour(() =>
		{
			if (Vector3.Distance(agentTrnasform.position, targetTransform.position) < explodeDistance)
			{
				OnTrigger.Invoke(Agent.Flags.OnTargetReach.ToString());
			}

			if (Vector3.Distance(agentTrnasform.position, targetTransform.position) > lostDistance)
			{
				OnTrigger.Invoke(Agent.Flags.OnTargetLost.ToString());
			}
		});

		return behaviourActions;
	}
}