using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KuznickiAgents.StatePattern
{
	public class Fsm : IDisposable
{
	private const int UNASSINGED_TRANSITION = int.MinValue;

	private readonly (int destination, Action onTransition)[,] transitions;
	private int currentState;

	private readonly Dictionary<string, int> flagAlias;

	private readonly Dictionary<int, State> states;

	private readonly Dictionary<int, Func<object[]>> enterParameters;
	private readonly Dictionary<int, Func<object[]>> updateParameters;
	private readonly Dictionary<int, Func<object[]>> exitParameters;

	private readonly ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

	private BehaviourActions GetCurrentUpdateBehaviour => states[currentState].GetTickBehaviours(updateParameters[currentState]?.Invoke());
	private BehaviourActions GetCurrentOnEnterBehaviour => states[currentState].GetOnEnterBehaviours(enterParameters[currentState]?.Invoke());
	private BehaviourActions GetCurrentOnExitBehaviour => states[currentState].GetOnExitBehaviours(exitParameters[currentState]?.Invoke());

	public Fsm(int statesAmount, Dictionary<string, int> flagAlias)
	{
		this.flagAlias = flagAlias;
		transitions = new (int, Action onTransition)[statesAmount, flagAlias.Count];

		for (int x = 0; x < transitions.GetLength(0); x++)
		{
			for (int y = 0; y < transitions.GetLength(1); y++)
			{
				transitions[x, y] = (UNASSINGED_TRANSITION, null);
			}
		}

		states = new Dictionary<int, State>();

		enterParameters = new Dictionary<int, Func<object[]>>();
		updateParameters = new Dictionary<int, Func<object[]>>();
		exitParameters = new Dictionary<int, Func<object[]>>();

		currentState = int.MinValue;
	}

	public void ForceState(int state)
	{
		if (currentState != int.MinValue)
		{
			if (states.ContainsKey(state))
			{
				ExecuteBehaviour(GetCurrentOnExitBehaviour);
			}
		}
		
		currentState = state;

		ExecuteBehaviour(GetCurrentOnEnterBehaviour);
	}

	public void OnFlag(string flag)
	{
		int destinationState = transitions[currentState, flagAlias[flag]].destination;
		if (destinationState != UNASSINGED_TRANSITION)
		{
			ExecuteBehaviour(GetCurrentOnExitBehaviour);
			transitions[currentState, flagAlias[flag]].onTransition?.Invoke();
			currentState = destinationState;
			ExecuteBehaviour(GetCurrentOnEnterBehaviour);
		}
	}

	public void RegisterTransition(int fromState, string flag, int toState, Action onTransition = null)
	{
		transitions[fromState, flagAlias[flag]] = (toState, onTransition);
	}

	public void AddState<TState>(int stateIdentifier,
		Func<object[]> enterParametersPointer = null,
		Func<object[]> updateParametersPointer = null,
		Func<object[]> exitParametersPointer = null,
		params object[] constructionParameters)
		where TState : State
	{
		TState state = (TState)Activator.CreateInstance(typeof(TState), constructionParameters);
		state.OnTrigger += OnFlag;
		int stateValue = stateIdentifier;
		states.Add(stateValue, state);
		enterParameters.Add(stateValue, enterParametersPointer);
		updateParameters.Add(stateValue, updateParametersPointer);
		exitParameters.Add(stateValue, exitParametersPointer);
	}

	public void Update()
	{
		if (states.ContainsKey(currentState))
		{
			ExecuteBehaviour(GetCurrentUpdateBehaviour);
		}
	}

	private void ExecuteBehaviour(BehaviourActions behaviourActions)
	{
		if (behaviourActions.Equals(default(BehaviourActions)))
			return;

		int executionOrder = 0;

		while ((behaviourActions.MainThreadBehaviours != null && behaviourActions.MainThreadBehaviours.Count > 0) ||
			(behaviourActions.MultiThreadableBehaviours != null && behaviourActions.MultiThreadableBehaviours.Count > 0))
		{
			Task MultiThreadableBehaviour = new Task(() =>
			{
				if (behaviourActions.MultiThreadableBehaviours != null)
				{
					if (behaviourActions.MultiThreadableBehaviours.ContainsKey(executionOrder))
					{
						Parallel.ForEach(behaviourActions.MultiThreadableBehaviours[executionOrder], parallelOptions, (behaviour) =>
						{
							behaviour?.Invoke();
						});
						behaviourActions.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
					}
				}
			});

			MultiThreadableBehaviour.Start();

			if (behaviourActions.MainThreadBehaviours != null)
			{
				if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
				{
					foreach (Action mainThreadBehaviour in behaviourActions.MainThreadBehaviours[executionOrder])
					{
						mainThreadBehaviour?.Invoke();
					}
					behaviourActions.MainThreadBehaviours.Remove(executionOrder);
				}
			}

			MultiThreadableBehaviour?.Wait();
			executionOrder++;
		}

		behaviourActions.TransitionBehaviour?.Invoke();
	}

	public void Dispose()
	{
		foreach (State state in states.Values)
		{
			state.OnTrigger -= OnFlag;
		}

		for (int i = 0; i < transitions.GetLength(0); i++)
		{
			for (int j = 0; j < transitions.GetLength(1); j++)
			{
				transitions[i, j].onTransition = null;
			}
		}

		for (int i = 0; i < enterParameters.Count; i++)
		{
			enterParameters[i] = null;
		}

		for (int i = 0; i < updateParameters.Count; i++)
		{
			updateParameters[i] = null;
		}

		for (int i = 0; i < exitParameters.Count; i++)
		{
			exitParameters[i] = null;
		}
	}
}
}

