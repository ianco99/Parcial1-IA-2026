using System;

namespace KuznickiAgents.StatePattern
{
	public abstract class State
	{
		public Action<string> OnTrigger;
		public abstract BehaviourActions GetOnEnterBehaviours(params object[] parameters);
		public abstract BehaviourActions GetTickBehaviours(params object[] parameters);
		public abstract BehaviourActions GetOnExitBehaviours(params object[] parameters);
	}
}
