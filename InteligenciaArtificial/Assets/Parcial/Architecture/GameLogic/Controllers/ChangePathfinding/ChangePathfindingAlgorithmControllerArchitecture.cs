using System;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.Pathfinding;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.ChangePathfinding.Events;
using Parcial.Architecture.GameLogic.Entities.LivingEntity;
using Parcial.Architecture.GameLogic.Entities.Systems;

namespace Parcial.Architecture.GameLogic.Controllers.ChangePathfinding
{
	public class ChangePathfindingAlgorithmControllerArchitecture : IDisposable
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		public ChangePathfindingAlgorithmControllerArchitecture()
		{
			EventBus.Subscribe<ChangePathfindingAlgorithmRequestEvent>(OnChangePathfindingAlgorithmRequest);
		}

		private void OnChangePathfindingAlgorithmRequest(in ChangePathfindingAlgorithmRequestEvent callback)
		{
			if (!EntityRegistry.Contains(callback.entityId) || EntityRegistry[callback.entityId] is not LivingEntity)
			{
				GameConsole.Warning($"Pathfinding change rejected: entity {callback.entityId} is not a living entity");
				return;
			}

			if (!Enum.IsDefined(typeof(PathfindingAlgorithm), callback.algorithm))
			{
				GameConsole.Warning($"Pathfinding change rejected: unknown algorithm {callback.algorithm}");
				return;
			}

			EventBus.Raise<ChangePathfindingAlgorithmRequestAcceptedEvent>(callback.entityId, callback.algorithm);
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<ChangePathfindingAlgorithmRequestEvent>(OnChangePathfindingAlgorithmRequest);
		}
	}
}
