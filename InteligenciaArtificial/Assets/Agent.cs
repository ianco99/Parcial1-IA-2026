using KuznickiAgents.StatePattern;
using UnityEngine;

class Agent : MonoBehaviour
{
	//Creeper
	public enum States
	{
		Patrol= 1,
		Chase = 2,
		Explode = 3
	}

	public enum Flags
	{
		OnTargetReach,
		OnTargetNear,
		OnTargetLost
	}

	public Hsm hsm;

	public Transform target;
	public float speed;
	public float explodeDistance;
	public float lostDistance;

	public Transform wayPoint1;
	public Transform wayPoint2;
	public float chaseDistance;

	public void Start()
	{
		hsm = new Hsm();

		NodeID patrolStateID = hsm.AddState<PatrolState>((int)States.Patrol,
			tickParametersPointer: () => new object[] { wayPoint1, wayPoint2, transform, target, speed, chaseDistance, Time.deltaTime }
		);

		NodeID chaseSateID = hsm.AddState<ChaseState>((int)States.Chase,
			tickParametersPointer: () => new object[] { transform, target, speed, explodeDistance, lostDistance, Time.deltaTime }
		);

		NodeID explodeStateID = hsm.AddState<ExplodeState>((int)States.Explode);

		hsm.AddFlagAlias(Flags.OnTargetNear.ToString());
		hsm.AddFlagAlias(Flags.OnTargetReach.ToString());
		hsm.AddFlagAlias(Flags.OnTargetLost.ToString());

		hsm.RegisterTransition(patrolStateID, Flags.OnTargetNear.ToString(), chaseSateID, () => { Debug.Log("Te vi"); });
		hsm.RegisterTransition(chaseSateID, Flags.OnTargetReach.ToString(), explodeStateID);
		hsm.RegisterTransition(chaseSateID, Flags.OnTargetLost.ToString(), patrolStateID);

		hsm.ForceState(patrolStateID);
	}

	private void Update()
	{
		hsm.Tick();
	}
}