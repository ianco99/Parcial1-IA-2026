using System;
using System.Collections.Generic;
using KuznickiAgents.StatePattern;
using UnityEngine;

namespace Agents
{
	public class AgentHSM : MonoBehaviour
	{
		public enum States
		{
			Patrol,
			Chase,
			Explode,
			CountdownState
		}

		public enum Triggers
		{
			OnTargetReach,
			OnTargetNear,
			OnTargetLost,
			OnTimerEnd,
			OnChaseEnd
		}


		private Hsm _hsm;

		private void Start()
		{
			Dictionary<string, int> states = new Dictionary<string, int>();
			states.Add(States.Patrol.ToString(), 0);
			states.Add(States.Chase.ToString(), 1);
			states.Add(States.Explode.ToString(), 2);
			states.Add(States.CountdownState.ToString(), 3);

			Dictionary<string, int> flagAlias = new Dictionary<string, int>();

			flagAlias.Add(Triggers.OnTargetReach.ToString(), 0);
			flagAlias.Add(Triggers.OnTargetNear.ToString(), 1);
			flagAlias.Add(Triggers.OnTargetLost.ToString(), 2);
			flagAlias.Add(Triggers.OnTimerEnd.ToString(), 3);
			flagAlias.Add(Triggers.OnChaseEnd.ToString(), 4);

			//_hsm = new HSM(states.Count, flagAlias);

			//_hsm.AddState<PatrolState>(Convert.ToInt32(States.Patrol), new int[] { 0 });
			//_hsm.AddState<ChaseState>(Convert.ToInt32(States.Chase), new int[] { 0, 1 });
			//_hsm.AddState<ExplodeState>(Convert.ToInt32(States.Explode), new int[] { 0 });
			//_hsm.AddState<CountdownState>(Convert.ToInt32(States.CountdownState), new int[] { 0 });

		}

		private void Update()
		{
			
		}
	}
}