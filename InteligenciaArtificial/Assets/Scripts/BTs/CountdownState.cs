using KuznickiAgents.StatePattern;
using UnityEngine;

public sealed class CountdownState : State
{
	private Vector3 scaleUp;
	private Vector3 normalScale;
	private float direction;
	private float scaleTime;
	private float elapsedTime = 0.0f;
	private float totalTime = 0.0f;

	public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
	{
		Vector3 normalScale = (Vector3)parameters[0];
		Vector3 increment = (Vector3)parameters[1];
		float time = (float)parameters[2];

		BehaviourActions behaviourActions = new BehaviourActions();

		behaviourActions.AddMainThreadableBehaviour(0, () =>
		{
			this.normalScale = normalScale;
			this.scaleTime = time;
			scaleUp = increment;

			totalTime = 0.0f;

			direction = 1.0f;
		});

		return behaviourActions;
	}

	public override BehaviourActions GetTickBehaviours(params object[] parameters)
	{
		Transform entity = parameters[0] as Transform;
		Transform target = parameters[1] as Transform;
		float deltaTime = (float)parameters[2];
		float speed = (float)parameters[3];

		BehaviourActions behaviourActions = new BehaviourActions();

		behaviourActions.AddMainThreadableBehaviour(0, () =>
		{
			totalTime += deltaTime;
			elapsedTime += deltaTime * direction;

			float t = elapsedTime / scaleTime;

			entity.localScale = Vector3.Lerp(normalScale, scaleUp, t);


			if (elapsedTime >= scaleTime || elapsedTime < 0.0f)
			{
				direction *= -1.0f;

				if (elapsedTime >= scaleTime)
					elapsedTime = scaleTime;
				else
					elapsedTime = 0.0f;
			}

			entity.position += (target.position - entity.position).normalized * speed * deltaTime;
		});

		behaviourActions.SetTransitionBehaviour(() =>
		{
			if (totalTime >= 3.0f)
			{
				//OnTrigger?.Invoke(Agent.Triggers.OnTimerEnd.ToString());
			}
		});

		return behaviourActions;
	}

	public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
	{
		return new BehaviourActions();
	}
}