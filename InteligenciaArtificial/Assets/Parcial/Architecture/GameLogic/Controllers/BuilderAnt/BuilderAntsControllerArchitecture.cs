using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Food;
using Parcial.Architecture.GameLogic.Entities.Systems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ianco99.ToolBox.DataFlow;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.BuilderAnt
{
	public class BuilderAntsControllerArchitecture : ITickable, IDisposable
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();

		List<Entities.BuilderAnt> builderAnts = new List<Entities.BuilderAnt>();
		
		public BuilderAntsControllerArchitecture()
		{
			EventBus.Subscribe<RequestCollectFoodEvent>(OnRequestCollectFood);
			EventBus.Subscribe<RequestLeaveFoodEvent>(OnRequestLeaveFood);
		}

		private readonly object foodLock = new object();

		private void OnRequestLeaveFood(in RequestLeaveFoodEvent callback)
		{
			lock (foodLock)
			{
				LeaveFood(callback.depositor, callback.queenID, callback.depositorTeamId);
			}
		}

		private void LeaveFood(uint depositor, uint queenID, int depositorTeamId)
		{
			Entities.BuilderAnt ant = (Entities.BuilderAnt)EntityRegistry[depositor];

			if (!ant.HasFood)
				return;

			Point queenPosition = EntityRegistry[queenID].Coordinate.Origin;
			Point[] queenNeighbors = Scene.GetNeighboringTiles(queenPosition);

			if (!Scene.TryGetFoodDeposit(queenNeighbors, depositorTeamId, out Food deposit, out Point freeTile))
				return;

			if (deposit != null)
			{
				deposit.Stack();
			}
			else
			{
				EventBus.Raise<SpawnRequestAcceptedEvent<Food>>(EntitiesLogic.GetFoodBlueprint(), new Coordinate(freeTile),
					TableNames.ANTS_TABLE_NAME, depositorTeamId);
			}

			ant.DropFood();

			EventBus.Raise<FoodDepositedEvent>(depositorTeamId, 1);
		}

		private void OnRequestCollectFood(in RequestCollectFoodEvent callback)
		{
			lock (foodLock)
			{
				if (!EntityRegistry.Contains(callback.foodID))
					return;

				((Entities.BuilderAnt)EntityRegistry[callback.collector]).CollectFood(callback.foodID);

				((Food)EntityRegistry[callback.foodID]).GetCollected();
			}
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<RequestCollectFoodEvent>(OnRequestCollectFood);
			EventBus.UnSubscribe<RequestLeaveFoodEvent>(OnRequestLeaveFood);
		}

		public void Tick(float deltaTime)
		{
			builderAnts.Clear();
			builderAnts.AddRange(EntityRegistry.BuilderAnts);
			
			Parallel.ForEach(builderAnts, (builderAnt) =>
			{
				builderAnt.Tick(deltaTime);
			});
		}
	}
}