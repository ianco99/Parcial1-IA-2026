using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Instances.QueenAnt.States
{
	public class SpawnEggState : State
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Entities.QueenAnt queenAnt;

		private bool isAwaitingResponse;
		private bool isRequestAccepted;
		private bool isRequestDeclined;

		public SpawnEggState(params object[] parameters)
		{
			queenAnt = (Entities.QueenAnt)parameters[0];
		}

		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions =  new BehaviourActions();

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				isAwaitingResponse = false;
				isRequestAccepted = false;
				isRequestDeclined = false;

				EventBus.Subscribe<EntityCreatedEvent<EggAnt.EggAnt>>(OnEggCreated);
				EventBus.Subscribe<SpawnRequestRejectedEvent<EggAnt.EggAnt>>(OnSpawnRequestDeclined);
			});

			return behaviourActions;
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				if (isAwaitingResponse || isRequestAccepted)
					return;

				if (isRequestDeclined && !TryRefreshEggSpawnPoint())
					return;

				if (isRequestDeclined)
					queenAnt.eggHatchType = queenAnt.DecideEggType();

				isRequestDeclined = false;

				isAwaitingResponse = true;

				EventBus.Raise<SpawnRequestEvent<EggAnt.EggAnt>>(EntitiesLogic.GetEggAntBlueprint(),
					new Coordinate(queenAnt.eggSpawnPoint), queenAnt.teamID);
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

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				EventBus.UnSubscribe<EntityCreatedEvent<EggAnt.EggAnt>>(OnEggCreated);
				EventBus.UnSubscribe<SpawnRequestRejectedEvent<EggAnt.EggAnt>>(OnSpawnRequestDeclined);
			});

			return behaviourActions;
		}

		private void OnEggCreated(in EntityCreatedEvent<EggAnt.EggAnt> callback)
		{
			if (callback.entityTeamID != queenAnt.teamID || !isAwaitingResponse)
				return;

			EntityRegistry.GetAs<EggAnt.EggAnt>(callback.EntityCreatedId).HatchType = queenAnt.eggHatchType;

			isAwaitingResponse = false;
			isRequestAccepted = true;
		}

		private void OnSpawnRequestDeclined(in SpawnRequestRejectedEvent<EggAnt.EggAnt> callback)
		{
			if (callback.teamId != queenAnt.teamID || !isAwaitingResponse)
				return;

			isAwaitingResponse = false;
			isRequestDeclined = true;
		}

		private bool TryRefreshEggSpawnPoint()
		{
			Coordinate anthill = Scene.GetSpawnTeamControllerArchitecture(queenAnt.teamID)
				.SpawnAnthillControllerArchitecture.MyAnthill;

			Point point = Scene.GetEmptyAnthillPoint(anthill, queenAnt.teamID);
			if (point == default)
				return false;

			queenAnt.eggSpawnPoint = point;
			return true;
		}
	}
}
