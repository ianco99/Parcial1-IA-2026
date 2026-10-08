using System;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Anthill;
using Parcial.Architecture.GameLogic.Controllers.ClaimTile;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.Team
{
	internal sealed class SpawnTeamControllerArchitecture : IInitable, IDisposable
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private const int UNCLAIMED_TILE_TEAM_ID = 0;

		private SpawnAnthillControllerArchitecture spawnAnthillControllerArchitecture;
		private SpawnQueenControllerArchitecture spawnQueenControllerArchitecture;
		private SpawnEggControllerArchitecture spawnEggControllerArchitecture;
		private SpawnBuilderAntControllerArchitecture spawnBuilderAntControllerArchitecture;
		private SpawnWarriorAntControllerArchitecture spawnWarriorAntControllerArchitecture;
		private ClaimTileControllerArchitecture claimTileControllerArchitecture;
		
		public SpawnAnthillControllerArchitecture SpawnAnthillControllerArchitecture => spawnAnthillControllerArchitecture;
		public SpawnQueenControllerArchitecture SpawnQueenControllerArchitecture => SpawnQueenControllerArchitecture;
		public ClaimTileControllerArchitecture ClaimTileControllerArchitecture => claimTileControllerArchitecture;
		
		private int id;
		private int depositedFood;

		public int DepositedFood => depositedFood;

		public SpawnTeamControllerArchitecture(int id)
		{
			this.id = id;

			EventBus.Subscribe<FoodDepositedEvent>(OnFoodDeposited);
			EventBus.Subscribe<QueenKilledEvent>(OnQueenKilled);

			spawnQueenControllerArchitecture = new SpawnQueenControllerArchitecture(id);
			spawnAnthillControllerArchitecture = new SpawnAnthillControllerArchitecture(id);
			spawnEggControllerArchitecture = new SpawnEggControllerArchitecture(id);
			spawnBuilderAntControllerArchitecture = new SpawnBuilderAntControllerArchitecture(id);
			spawnWarriorAntControllerArchitecture = new SpawnWarriorAntControllerArchitecture(id);
			claimTileControllerArchitecture = new ClaimTileControllerArchitecture(id);
		}
		
		private void OnQueenKilled(in QueenKilledEvent callback)
		{
			if (callback.teamId != id)
				return;

			foreach (Entity builderAnt in EntityRegistry.BuilderAnts)
			{
				if (builderAnt.teamID == id)
					EntityRegistry.UnRegister(builderAnt);
			}

			foreach (Entity eggAnt in EntityRegistry.EggAnts)
			{
				if (eggAnt.teamID == id)
					EntityRegistry.UnRegister(eggAnt);
			}

			foreach (Entity warriorAnt in EntityRegistry.WarriorAnts)
			{
				if (warriorAnt.teamID == id)
					EntityRegistry.UnRegister(warriorAnt);
			}

			foreach (Point tile in Scene.GetTeamAnthillTiles(id))
				EventBus.Raise<ModifyTerrainRequestEvent>(tile, tile, Scene.DefaultTileDefinition, UNCLAIMED_TILE_TEAM_ID);

			GameConsole.Log($"Team {id} was defeated: queen {callback.queenID} died");
		}

		private void OnFoodDeposited(in FoodDepositedEvent callback)
		{
			if (callback.teamId != id)
				return;

			depositedFood += callback.amount;
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<FoodDepositedEvent>(OnFoodDeposited);
			EventBus.UnSubscribe<QueenKilledEvent>(OnQueenKilled);
			spawnQueenControllerArchitecture.Dispose();
			spawnAnthillControllerArchitecture.Dispose();
			spawnEggControllerArchitecture.Dispose();
			spawnBuilderAntControllerArchitecture.Dispose();
			spawnWarriorAntControllerArchitecture.Dispose();
			claimTileControllerArchitecture.Dispose();
		}

        public void Init()
        {
			spawnBuilderAntControllerArchitecture.Init();
			spawnWarriorAntControllerArchitecture.Init();
        }

        public void LateInit()
        {
			spawnBuilderAntControllerArchitecture.LateInit();
			spawnWarriorAntControllerArchitecture.LateInit();
        }
    }
}