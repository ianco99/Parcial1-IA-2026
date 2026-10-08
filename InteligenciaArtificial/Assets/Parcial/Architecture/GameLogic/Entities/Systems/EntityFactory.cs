using System;
using System.Collections.Generic;
using System.Reflection;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Systems
{
	public sealed class EntityFactory : IService, IDisposable
	{
		public bool IsPersistance { get; }

		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();

		private uint lastAssignedEntityId;
		private readonly object idLock = new object();

		private Dictionary<Type, ConstructorInfo> entityConstructors;
		private MethodInfo registerEntityMethod;
		private MethodInfo raiseEntityCreatedMethod;

		private Dictionary<Type, object> creationSubscriptions;
		private MethodInfo subscribeToCreationMethod;
		private MethodInfo unsubscribeMethod;

		public EntityFactory()
		{
			this.lastAssignedEntityId = Entity.UNASSIGNED_ENTITY_ID;
			entityConstructors = new Dictionary<Type, ConstructorInfo>();
			creationSubscriptions = new Dictionary<Type, object>();

			registerEntityMethod = EntityRegistry.GetType().GetMethod(EntityRegistry.RegisterMethodName,
				BindingFlags.NonPublic | BindingFlags.Instance);

			raiseEntityCreatedMethod = GetType()
				.GetMethod(nameof(RaiseEntityCreated), BindingFlags.NonPublic | BindingFlags.Instance);

			subscribeToCreationMethod = GetType().GetMethod(nameof(SubscribeToCreation),
				BindingFlags.NonPublic | BindingFlags.Instance);
			unsubscribeMethod = typeof(EventBus).GetMethod(nameof(EventBus.UnSubscribe),
				BindingFlags.Public | BindingFlags.Instance);
			RegisterEntityMethods();
		}

		private void SubscribeToCreation<TEntityType>() where TEntityType : Entity
		{
			EventBus.EventCallback<SpawnRequestAcceptedEvent<TEntityType>> callback =
				EventBus.SubscribeAndReturn<SpawnRequestAcceptedEvent<TEntityType>>(SpawnEntity);
			creationSubscriptions.Add(typeof(SpawnRequestAcceptedEvent<TEntityType>), callback);

			void SpawnEntity(in SpawnRequestAcceptedEvent<TEntityType> callback)
			{
				CreateInstance<TEntityType>(callback.BlueprintToSpawn,
					callback.CoordinateToSpawn,
					callback.BlueprintFile,
					callback.teamID);
			}
		}

		private void UnsubscribeToCreation()
		{
			foreach (KeyValuePair<Type, object> methodsToUnsubscribe in creationSubscriptions)
			{
				unsubscribeMethod.MakeGenericMethod(methodsToUnsubscribe.Key)
					.Invoke(EventBus, new object[] { methodsToUnsubscribe.Value });
			}
		}

		public void CreateInstance<TEntityType>(string blueprintId, Coordinate coordinate, string fileName, int teamId)
			where TEntityType : Entity
		{
			uint newEntityId;

			lock (idLock)
			{
				lastAssignedEntityId++;
				newEntityId = lastAssignedEntityId;
			}

			if (!entityConstructors.ContainsKey(typeof(TEntityType)))
				throw new MissingMethodException($"missing constructor for {typeof(TEntityType).Name}");

			object newEntity = entityConstructors[typeof(TEntityType)].Invoke(new object[] { newEntityId, coordinate, teamId });

			BlueprintBinder.Apply(ref newEntity, fileName, blueprintId);

			(newEntity as TEntityType).Init();

			if (registerEntityMethod == null)
				throw new MissingMethodException($"Missing EntityRegistry register method");

			registerEntityMethod.Invoke(EntityRegistry, new object[] { newEntity });

			List<Type> entityTypes = new List<Type>();
			Type currentType = null;

			do
			{
				currentType = currentType == null ? newEntity.GetType() : currentType.BaseType;
				entityTypes.Add(currentType);
			} while (currentType != typeof(Entity));

			for (int i = entityTypes.Count - 1; i >= 0; i--)
			{
				raiseEntityCreatedMethod.MakeGenericMethod(entityTypes[i])
					.Invoke(this, new object[] { blueprintId, fileName, newEntity, teamId });
			}
			
			(newEntity as TEntityType).LateInit();
		}

		private void RaiseEntityCreated<TEntityType>(string blueprintId, string blueprintFile, TEntityType newEntity, int teamID) where TEntityType : Entity
		{
			EventBus.Raise<EntityCreatedEvent<TEntityType>>(blueprintId, blueprintFile, newEntity.ID,
				newEntity.Coordinate , teamID);
		}

		private void RegisterEntityMethods()
		{
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					if (typeof(Entity).IsAssignableFrom(type))
					{
						RegisterEntity(type);
						subscribeToCreationMethod.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
					}
				}
			}

			void RegisterEntity(Type entityType)
			{
				foreach (ConstructorInfo constructor in entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					ParameterInfo[] parameters = constructor.GetParameters();

					if (parameters.Length == 3 &&
					    parameters[0].ParameterType == typeof(uint) &&
					    parameters[1].ParameterType == typeof(Coordinate) &&
					    parameters[2].ParameterType == typeof(int))
					{
						entityConstructors.Add(entityType, constructor);
						break;
					}
				}
			}
		}
		
		public void Dispose()
		{
			UnsubscribeToCreation();
		}
	}
}