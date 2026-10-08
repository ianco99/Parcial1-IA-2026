using System;
using System.Collections.Generic;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.Flocking;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.LivingEntity;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity.Events;
using Parcial.View.Mapping;
using ZooArchitect.Architecture.Entities.Events;

namespace Parcial.View.GameLogic.Entities.Systems
{
    [ViewOf(typeof(EntityRegistry))]
    internal sealed class EntityRegistryView : IService, IDisposable
    {
        public bool IsPersistance => false;

        private Dictionary<uint, EntityView> entities;
        private Dictionary<Type, List<uint>> entityIdsPerType;
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        EntitiesFlocking EntitiesFlocking => ServiceProvider.Instance.GetService<EntitiesFlocking>();

        public EntityView this[uint ID] => entities[ID];
        
        public EntityRegistryView()
        {
            entities = new Dictionary<uint, EntityView>();
            entityIdsPerType = new Dictionary<Type, List<uint>>();
            EventBus.SubscribeOnlyMainThread<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        private void OnEntityDestroyed(in EntityDestroyedEvent callback)
        {
            if (!entities.TryGetValue(callback.entityID, out EntityView entityView))
                return;

            if (entityView is IFlockingAgent flockingAgent)
                EntitiesFlocking.RemoveAgent(flockingAgent);

            for (Type type = entityView.GetType(); type != typeof(EntityView); type = type.BaseType)
                entityIdsPerType[type].Remove(callback.entityID);

            entityView.Dispose();
            entities.Remove(callback.entityID);
            UnityEngine.Object.Destroy(entityView.gameObject);
        }

        internal string RegisterMethodName => nameof(Register);

        private void Register(EntityView entityView)
        {
            entities.Add(entityView.ArchitectureEnitityID, entityView);
            Type currentEntityType = entityView.GetType();
            
            
            do
            {
                if (!entityIdsPerType.ContainsKey(currentEntityType))
                    entityIdsPerType.Add(currentEntityType, new List<uint>());
                entityIdsPerType[currentEntityType].Add(entityView.ArchitectureEnitityID);
                currentEntityType = currentEntityType.BaseType;
            } while (currentEntityType != typeof(EntityView));

            if (entityView is IFlockingAgent)
                EventBus.Raise<FlockingAgentRegisteredEvent>(entityView.ArchitectureEnitityID);
        }

        public bool Contains(uint ID) => entities.ContainsKey(ID);

        public EntityType GetAs<EntityType>(uint ID) where EntityType : EntityView
        {
            if (ID == Entity.UNASSIGNED_ENTITY_ID)
            {
                throw new NullReferenceException("Entity id 0 represents a null entity");
            }

            if (!entities.ContainsKey(ID))
            {
                throw new KeyNotFoundException(ID.ToString());
            }

            if (entities[ID] is not EntityType)
            {
                throw new InvalidCastException($"An attempt was made to obtain a type {entities[ID].GetType().Name}"
                                             + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");
            }

            return entities[ID] as EntityType;
        }

        public IEnumerable<EntityView> Entities => FilterEntities<EntityView>();
        public IEnumerable<LivingEntityView> LivingEntities => FilterEntities<LivingEntityView>();

        public IEnumerable<EntityType> FilterEntities<EntityType>() where EntityType : EntityView
        {
            if (entityIdsPerType.ContainsKey(typeof(EntityType)))
            {
                foreach (uint ID in entityIdsPerType[typeof(EntityType)])
                {
                    yield return entities[ID] as EntityType;
                }
            }
        }

        public void Dispose()
        {
            EventBus.UnsubscribeOnlyMainThread<EntityDestroyedEvent>(OnEntityDestroyed);
        }
    }
}