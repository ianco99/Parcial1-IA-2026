using System;
using System.Collections.Generic;
using KuznickiAgents.Pathfinding;
using Parcial.Architecture.GameLogic.Controllers.ChangePathfinding;
using Parcial.Architecture.GameLogic.Controllers.ChangePathfinding.Events;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.Math;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;
using Parcial.View.Mapping;
using UnityEngine;

namespace Parcial.View.GameLogic.Controllers
{
	[ViewOf(typeof(ChangePathfindingAlgorithmControllerArchitecture))]
	internal sealed class ChangePathfindingAlgorithmControllerView : SingleSelectionControllerView
	{
		private uint selectedEntityId = Entity.UNASSIGNED_ENTITY_ID;

		protected override List<string> GetValidBlueprints(Point clickPoint)
		{
			selectedEntityId = GetClosestAntToMouse();

			if (selectedEntityId == Entity.UNASSIGNED_ENTITY_ID)
				return new List<string>();

			return new List<string>(Enum.GetNames(typeof(PathfindingAlgorithm)));
		}

		protected override string GetTitle(Point clickPoint)
		{
			LivingEntityView antView = EntityRegistryView.GetAs<LivingEntityView>(selectedEntityId);
			return $"{antView.GetType().Name.Replace("View", string.Empty)} #{selectedEntityId} (team {antView.EntityTeamID})";
		}

		protected override Dictionary<string, Action> GetActionsToDisplay(Point clickPoint, List<string> blueprints)
		{
			uint entityId = selectedEntityId;
			PathfindingAlgorithm current = EntityRegistry
				.GetAs<Architecture.GameLogic.Entities.LivingEntity.LivingEntity>(entityId).PathfindingAlgorithm;

			Dictionary<string, Action> changeAlgorithm = new Dictionary<string, Action>();
			for (int i = 0; i < blueprints.Count; i++)
			{
				PathfindingAlgorithm algorithm = (PathfindingAlgorithm)Enum.Parse(typeof(PathfindingAlgorithm), blueprints[i]);
				string label = algorithm == current ? $"{blueprints[i]} (current)" : blueprints[i];

				changeAlgorithm.Add(label, () =>
				{
					if (EntityRegistryView.Contains(entityId))
						EventBus.Raise<ChangePathfindingAlgorithmRequestEvent>(entityId, algorithm);
				});
			}
			return changeAlgorithm;
		}

		private uint GetClosestAntToMouse()
		{
			Vector3 mousePosition = GameScene.GetMouseWorldPosition();
			Vector2 cellSize = GameScene.GetCellSize;
			float closestDistance = Mathf.Max(cellSize.x, cellSize.y);
			uint closestId = Entity.UNASSIGNED_ENTITY_ID;

			foreach (LivingEntityView antView in EntityRegistryView.LivingEntities)
			{
				float distance = Vector2.Distance(antView.transform.position, mousePosition);
				if (distance <= closestDistance)
				{
					closestDistance = distance;
					closestId = antView.ArchitectureEnitityID;
				}
			}

			return closestId;
		}

		public override void Dispose()
		{
		}
	}
}
