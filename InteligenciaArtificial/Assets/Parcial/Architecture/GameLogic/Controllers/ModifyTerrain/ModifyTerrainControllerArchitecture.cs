using System;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.ModifyTerrain
{
	public class ModifyTerrainControllerArchitecture : IDisposable
	{
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		public ModifyTerrainControllerArchitecture()
		{
			EventBus.Subscribe<ModifyTerrainRequestEvent>(ModifyTerrainRequest);
			EventBus.Subscribe<ModifyTerrainEXRequestEvent>(ModifyTerrainEXRequest);
		}

		private void ModifyTerrainRequest(in ModifyTerrainRequestEvent callback)
		{
			Coordinate area = new Coordinate(callback.origin, callback.end);
			bool isRelease = callback.newTileId == Scene.DefaultTileDefinition && Scene.IsCoordinateInsideMap(area);

			if (isRelease || Scene.IsDefaultArea(area))
			{
				EventBus.Raise<ModifyTerrainRequestAcceptedEvent>(callback.origin, callback.end, callback.newTileId,
					callback.terrainTeamId);
			}
			else
			{
				GameConsole.Error("Can't place area on this section of map.");
			}
		}

		private void ModifyTerrainEXRequest(in ModifyTerrainEXRequestEvent callback)
		{
			EventBus.Raise<ModifyTerrainEXRequestAcceptedEvent>(callback.origin, callback.end, callback.tileExceptions,
				callback.newTileId, callback.terrainTeamId);
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<ModifyTerrainRequestEvent>(ModifyTerrainRequest);
			EventBus.UnSubscribe<ModifyTerrainEXRequestEvent>(ModifyTerrainEXRequest);
		}
	}
}