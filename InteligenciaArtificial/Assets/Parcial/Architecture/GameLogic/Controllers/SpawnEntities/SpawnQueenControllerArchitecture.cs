using System;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Anthill.Events;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities
{
	internal sealed class SpawnQueenControllerArchitecture : IDisposable
	{
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
        EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();

		private int teamId;

		internal SpawnQueenControllerArchitecture(int id)
		{
			teamId = id;

			EventBus.Subscribe<AnthillSpawnedEvent>(OnAnthillSpawned);
			EventBus.Subscribe<SpawnRequestEvent<QueenAnt>>(OnQueenAntRequest);
		}

		private void OnQueenAntRequest(in SpawnRequestEvent<QueenAnt> callback)
		{
			if (callback.teamId != teamId)
				return;

			EventBus.Raise<SpawnRequestAcceptedEvent<QueenAnt>>(callback.blueprintToSpawn, callback.coordinateToSpawn,
				TableNames.ANTS_TABLE_NAME, teamId);
		}

		private void OnAnthillSpawned(in AnthillSpawnedEvent callback)
		{
			if (callback.teamID != teamId)
				return;

			string[] data = BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME).ToArray();
			EventBus.Raise<SpawnRequestEvent<QueenAnt>>(EntitiesLogic.GetQueenAntBlueprint(), new Coordinate(callback.center), teamId);
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<AnthillSpawnedEvent>(OnAnthillSpawned);
			EventBus.UnSubscribe<SpawnRequestEvent<QueenAnt>>(OnQueenAntRequest);
		}
	}
}