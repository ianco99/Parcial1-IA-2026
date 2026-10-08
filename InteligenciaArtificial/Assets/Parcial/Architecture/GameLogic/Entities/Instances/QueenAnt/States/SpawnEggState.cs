using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using System;

namespace Parcial.Architecture.GameLogic.Entities.Instances.QueenAnt.States
{
	public class SpawnEggState : State
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();





		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions =  new BehaviourActions();

			Action<bool> awaitingResponse = parameters[0] as Action<bool>;
			Action<bool> requestAccepted = parameters[1] as Action<bool>;
			Action<bool> requestDeclined = parameters[2] as Action<bool>;

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				awaitingResponse.Invoke(false);
                requestAccepted.Invoke(false);
                requestDeclined.Invoke(false);
			});

			return behaviourActions;
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			Point eggPoint = (Point)parameters[0];
			int teamID = (int)parameters[1];
			
			Action retryEggType = (Action)parameters[2];

            Action<bool> awaitingResponse = parameters[3] as Action<bool>;
            Action<bool> requestDeclined = parameters[4] as Action<bool>;

            Func<bool> TryEggPoint = parameters[5] as Func<bool>;
			bool isAwaitingResponse = (bool)parameters[6];
			bool isRequestAccepted = (bool)parameters[7];
			bool isRequestDeclined = (bool)parameters[8];

            behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				if (isAwaitingResponse || isRequestAccepted)
					return;

				if (isRequestDeclined && !TryEggPoint.Invoke())
					return;

				if (isRequestDeclined)
					retryEggType?.Invoke();

                requestDeclined.Invoke(false);

				awaitingResponse.Invoke(true);

				EventBus.Raise<SpawnRequestEvent<EggAnt.EggAnt>>(EntitiesLogic.GetEggAntBlueprint(),
					new Coordinate(eggPoint), teamID);
			});

			behaviourActions.SetTransitionBehaviour(() =>
			{
				if (isRequestAccepted)
					OnTrigger?.Invoke(Entities.QueenAnt.Flags.OnBackToIdle.ToString());
			});

			return behaviourActions;
		}

		public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			return behaviourActions;
		}
	}
}
