using System;
using System.Collections.Concurrent;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Scheduling;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.ClaimTile
{
	public class ClaimTileControllerArchitecture : IDisposable
	{
		public const int CLAIM_FOOD_REQUIRED = 10;
		public const float CLAIM_COOLDOWN = 10.0f;

		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();

		private readonly ConcurrentQueue<Point> pendingClaims = new ConcurrentQueue<Point>();

		private int id;
		private bool isOnCooldown;

		public bool IsOnCooldown => isOnCooldown;
		public bool HasFoodToClaim => Scene.GetStoredFoodAmount(id) >= CLAIM_FOOD_REQUIRED;
		public bool CanClaim => !isOnCooldown && HasFoodToClaim;

		public ClaimTileControllerArchitecture(int id)
		{
			this.id = id;

			EventBus.Subscribe<RequestClaimTileEvent>(OnRequestClaimTile);
		}

		private void OnRequestClaimTile(in RequestClaimTileEvent callback)
		{
			if (callback.claimerTeamId != id)
				return;

			pendingClaims.Enqueue(callback.tile);
		}

		public void ResolveClaims()
		{
			while (pendingClaims.TryDequeue(out Point tile))
			{
				if (!CanClaim || !Scene.IsClaimableTile(tile, id))
					continue;

				isOnCooldown = true;
				TaskScheduler.Schedule(() => isOnCooldown = false, CLAIM_COOLDOWN);

				EventBus.Raise<ModifyTerrainRequestEvent>(tile, tile, Scene.AnthillTileDefinition, id);
			}
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<RequestClaimTileEvent>(OnRequestClaimTile);
		}
	}
}
