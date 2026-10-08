using ianco99.ToolBox.Events;
using ianco99.ToolBox.Scheduling;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.EggAnt;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using System;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities
{
	public class SpawnEggControllerArchitecture : IDisposable
	{
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();

		private int teamID;
		public SpawnEggControllerArchitecture(int teamId)
		{
			teamID = teamId;

			EventBus.Subscribe<SpawnRequestEvent<EggAnt>>(OnEggSpawnRequest);
			EventBus.Subscribe<EntityCreatedEvent<EggAnt>>(OnEggCreated);
		}

		private void OnEggSpawnRequest(in SpawnRequestEvent<EggAnt> callback)
		{
			if (callback.teamId != teamID)
				return;

			if (!Scene.IsCoordinateEmpty(callback.coordinateToSpawn))
			{
				EventBus.Raise<SpawnRequestRejectedEvent<EggAnt>>(callback.blueprintToSpawn,callback.coordinateToSpawn, callback.teamId, "Not an empty coordinate");
			}
			else if (!Scene.TryConsumeStoredFood(teamID, Entities.QueenAnt.EGG_FOOD_COST))
			{
				EventBus.Raise<SpawnRequestRejectedEvent<EggAnt>>(callback.blueprintToSpawn,callback.coordinateToSpawn, callback.teamId, "Not enough food");
			}
			else
			{
				EventBus.Raise<SpawnRequestAcceptedEvent<EggAnt>>(callback.blueprintToSpawn, callback.coordinateToSpawn,
					TableNames.ANTS_TABLE_NAME, callback.teamId);
			}
		}

		private void OnEggCreated(in EntityCreatedEvent<EggAnt> callback)
		{
			if (callback.entityTeamID != teamID)
				return;

			uint eggID = callback.EntityCreatedId;

			TaskScheduler.Schedule(() => Hatch(eggID), EggAnt.HATCH_TIME);
		}

		private void Hatch(uint eggID)
		{
			if (!EntityRegistry.Contains(eggID))
				return;

			EggAnt egg = EntityRegistry.GetAs<EggAnt>(eggID);
			Coordinate coordinate = egg.Coordinate;
			EggAnt.HatchTypes hatchType = egg.HatchType;

			EntityRegistry.UnRegister(egg);

			if (hatchType == EggAnt.HatchTypes.WarriorAnt)
				EventBus.Raise<SpawnRequestEvent<Entities.Instances.WarriorAnt.WarriorAnt>>(EntitiesLogic.GetWarriorAntBlueprint(), coordinate, teamID);
			else
				EventBus.Raise<SpawnRequestEvent<Entities.BuilderAnt>>(EntitiesLogic.GetBuilderAntBlueprint(), coordinate, teamID);

			GameConsole.Log($"Team {teamID}: egg {eggID} hatched into a {hatchType}");
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<SpawnRequestEvent<EggAnt>>(OnEggSpawnRequest);
			EventBus.UnSubscribe<EntityCreatedEvent<EggAnt>>(OnEggCreated);
		}
	}
}
