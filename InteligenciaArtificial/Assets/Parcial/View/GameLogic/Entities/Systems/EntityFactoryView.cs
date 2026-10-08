using System;
using System.Collections.Generic;
using System.Reflection;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.Mapping;
using Parcial.View.Resources;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Entities.Systems
{
	[ViewOf(typeof(EntityFactory))]
	internal class EntityFactoryView : IDisposable
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
		private PrefabsRegistryView PrefabsRegistryView => ServiceProvider.Instance.GetService<PrefabsRegistryView>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private GameScene GameScene => ServiceProvider.Instance.GetService<GameScene>();
		private BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();

		private MethodInfo registerEntityMethod;
		private MethodInfo setEntityIdMethod;
		private MethodInfo setEntityTeamMethod;
		
		public EntityFactoryView()
		{
			EventBus.SubscribeOnlyMainThread<EntityCreatedEvent<Entity>>(OnEntityCreated);
			
			registerEntityMethod = EntityRegistryView.GetType().GetMethod(EntityRegistryView.RegisterMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
			
			setEntityIdMethod = typeof(EntityView).GetMethod(EntityView.SetIdMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
			setEntityTeamMethod = typeof(EntityView).GetMethod(EntityView.SetTeamIdMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private void OnEntityCreated(in EntityCreatedEvent<Entity> entityCreatedEvent)
		{
			if (!EntityRegistry.Contains(entityCreatedEvent.EntityCreatedId))
				return;

			bool IsSingleCoordinate =EntityRegistry[entityCreatedEvent.EntityCreatedId].Coordinate.IsSingleCoordinate;
			List<object> parameters = new List<object>();

			GameObject prefab = PrefabsRegistryView.Get(
				TableNamesView.ArchitectureToView[entityCreatedEvent.BlueprintTable], entityCreatedEvent.BlueprintId);

			GameObject objectToInstance = IsSingleCoordinate ? prefab : null;
			
			string viewTable = TableNamesView.ArchitectureToView[entityCreatedEvent.BlueprintTable];

			object viewComponent = GameScene.AddSceneComponent(ViewArchitectureMap.ViewOf(EntityRegistry[entityCreatedEvent.EntityCreatedId].GetType()),
				PrefabsRegistryView.Get(viewTable, entityCreatedEvent.BlueprintId).name + $" - Architecture type: {EntityRegistry[entityCreatedEvent.EntityCreatedId].GetType().Name} - ID: {entityCreatedEvent.EntityCreatedId}",
				GameScene.EntitiesContainer.transform,
				objectToInstance);

			(viewComponent as ViewComponent).transform.position = GameScene.CoordinateToWorld(EntityRegistry[entityCreatedEvent.EntityCreatedId].Coordinate);

			BlueprintBinder.Apply(ref viewComponent, viewTable, PrefabsRegistryView.ArchitectureToViewId(viewTable, entityCreatedEvent.BlueprintId));

			if(IsSingleCoordinate)
			{
				SpriteRenderer spriteRenderer = (viewComponent as ViewComponent).GetComponent<SpriteRenderer>();
				spriteRenderer.sortingOrder = GameScene.ENTITIES_DRAWING_ORDER;
			}
			else
			{
				parameters.Add(prefab);
				parameters.Add(GameScene.ENTITIES_DRAWING_ORDER);
			}

			setEntityIdMethod.Invoke(viewComponent, new object[] { entityCreatedEvent.EntityCreatedId });

			setEntityTeamMethod.Invoke(viewComponent, new object[] { entityCreatedEvent.entityTeamID });
			
			(viewComponent as ViewComponent).Init(parameters.ToArray());

			registerEntityMethod.Invoke(EntityRegistryView, new object[] { viewComponent });

			(viewComponent as ViewComponent).LateInit();
		}

		public void Dispose()
		{
            EventBus.UnsubscribeOnlyMainThread<EntityCreatedEvent<Entity>>(OnEntityCreated);
		}
	}
}