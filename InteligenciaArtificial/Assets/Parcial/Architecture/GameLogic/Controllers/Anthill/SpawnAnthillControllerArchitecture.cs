using System;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Config;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Anthill.Events;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.Anthill
{
	public class SpawnAnthillControllerArchitecture : IDisposable
	{
		private const int ANTHILL_SIZE = 5;
		private const int ANTHILL_MARGIN = 1;
		private const int MAX_PLACEMENT_ATTEMPTS = 100;

		private static readonly int[] EMPTY_TILES = { 0, 1, 3, 4, 5, 9, 15, 19, 20, 21, 23, 24 };

		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		private readonly Random random = new Random();

		private int id;
		private Coordinate myAnthill;
		public Coordinate MyAnthill => myAnthill;

		public SpawnAnthillControllerArchitecture(int id)
		{
			this.id = id;
			
			EventBus.Subscribe<AnthillSpawnedEvent>(OnAnthillSpawned);
			
			SpawnAnthill();
		}

		private void OnAnthillSpawned(in AnthillSpawnedEvent callback)
		{
			if (callback.teamID != id)
				return;

			myAnthill = new Coordinate(callback.origin, callback.end);
		}

		private void SpawnAnthill()
		{
			if (!TryFindAnthillSpot(out Point start, out Point end))
			{
				GameConsole.Warning($"Could not find room for anthill {id}");
				return;
			}

			EventBus.Raise<ModifyTerrainEXRequestEvent>(start, end, EMPTY_TILES, "Anthill", id);
		}

		private bool TryFindAnthillSpot(out Point start, out Point end)
		{
			Coordinate mapCoordinate = Scene.MapCoordinate;
			int maxOriginX = mapCoordinate.maxX - (ANTHILL_SIZE - 1);
			int maxOriginY = mapCoordinate.maxY - (ANTHILL_SIZE - 1);

			if (maxOriginX >= mapCoordinate.minX && maxOriginY >= mapCoordinate.minY)
			{
				for (int attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS; attempt++)
				{
					start = new Point(random.Next(mapCoordinate.minX, maxOriginX + 1),
						random.Next(mapCoordinate.minY, maxOriginY + 1));
					end = new Point(start.x + ANTHILL_SIZE - 1, start.y + ANTHILL_SIZE - 1);

					if (Scene.CanPlaceArea(start, end, ANTHILL_MARGIN))
						return true;
				}
			}

			start = default;
			end = default;
			return false;
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<AnthillSpawnedEvent>(OnAnthillSpawned);
		}
	}
}