using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KuznickiAgents.StatePattern
{
	public struct BehaviourActions
	{
		private Dictionary<int, List<Action>> mainThreadBehaviours;
		private ConcurrentDictionary<int, List<Action>> multiThreadableBehaviours;
		private Action transitionBehaviour;

		public Dictionary<int, List<Action>> MainThreadBehaviours => mainThreadBehaviours;
		public ConcurrentDictionary<int, List<Action>> MultiThreadableBehaviours => multiThreadableBehaviours;
		public Action TransitionBehaviour => transitionBehaviour;

		public void AddMainThreadableBehaviour(int excecutionOrder, Action behaviour) 
		{
			if (mainThreadBehaviours == null)
				mainThreadBehaviours = new Dictionary<int, List<Action>>();
			if (!mainThreadBehaviours.ContainsKey(excecutionOrder))
				mainThreadBehaviours.TryAdd(excecutionOrder, new List<Action>());

			mainThreadBehaviours[excecutionOrder].Add(behaviour);
		}

		public void AddMultiThreadableBehaviour(int excecutionOrder, Action behaviour) 
		{
			if (multiThreadableBehaviours == null)
				multiThreadableBehaviours = new ConcurrentDictionary<int, List<Action>>();
			if (!multiThreadableBehaviours.ContainsKey(excecutionOrder))
				multiThreadableBehaviours.TryAdd(excecutionOrder, new List<Action>());

			multiThreadableBehaviours[excecutionOrder].Add(behaviour);
		}

		public void SetTransitionBehaviour(Action behaviour) 
		{
			transitionBehaviour = behaviour;
		}
	}
}