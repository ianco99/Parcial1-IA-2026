using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using System;
using System.Collections.Generic;
using ZooArchitect.Architecture.Entities.Events;

namespace Parcial.Architecture.GameLogic.Entities.Systems
{
    public sealed class EntityRegistry : IService
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        public bool IsPersistance => false;

        private Dictionary<uint, Entity> entities;
        private Dictionary<Type, List<uint>> entityIdsPerType;

        private readonly object registryLock = new object();

        public EntityRegistry()
        {
            entities = new Dictionary<uint, Entity>();
            entityIdsPerType = new Dictionary<Type, List<uint>>();
        }

        public Entity this[uint ID]
        {
            get
            {
                lock (registryLock)
                {
                    return entities[ID];
                }
            }
        }

        public bool Contains(uint ID)
        {
            lock (registryLock)
            {
                return entities.ContainsKey(ID);
            }
        }

        public bool TryGet(uint ID, out Entity entity)
        {
            lock (registryLock)
            {
                return entities.TryGetValue(ID, out entity);
            }
        }

        internal static string RegisterMethodName => nameof(Register);

        private void Register(Entity entity)
        {
            lock (registryLock)
            {
                entities.Add(entity.ID, entity);
                Type currentEntityType = null;
                do
                {
                    currentEntityType = currentEntityType == null ? entity.GetType() : currentEntityType.BaseType;
                    if (!entityIdsPerType.ContainsKey(currentEntityType))
                        entityIdsPerType.Add(currentEntityType, new List<uint>());
                    entityIdsPerType[currentEntityType].Add(entity.ID);
                } while (currentEntityType != typeof(Entity));
            }
        }

        public void UnRegister(Entity entity)
        {
            lock (registryLock)
            {
                if (entities.ContainsKey(entity.ID))
                {
                    EventBus.Raise<EntityDestroyedEvent>(entity.ID);
                    entities.Remove(entity.ID);

                    Type currentEntityType = null;

                    do
                    {
                        currentEntityType = currentEntityType == null ? entity.GetType() : currentEntityType.BaseType;
                        if (!entityIdsPerType.ContainsKey(currentEntityType))
                            entityIdsPerType.Add(currentEntityType, new List<uint>());
                        entityIdsPerType[currentEntityType].Remove(entity.ID);
                    } while (currentEntityType != typeof(Entity));
                }
            }
        }

        public EntityType GetAs<EntityType>(uint ID) where EntityType : Entity
        {
            if (ID == Entity.UNASSIGNED_ENTITY_ID)
            {
                throw new NullReferenceException("Entity id 0 represents a null entity");
            }

            Entity entity;

            lock (registryLock)
            {
                if (!entities.TryGetValue(ID, out entity))
                {
                    throw new KeyNotFoundException(ID.ToString());
                }
            }

            if (entity is not EntityType)
            {
                throw new InvalidCastException($"An attempt was made to obtain a type {entity.GetType().Name}"
                                               + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");
            }

            return entity as EntityType;
        }
        
        public IEnumerable<Entity> Entities => FilterEntities<Entity>();
        public IEnumerable<Entity> EggAnts => FilterEntities<EggAnt.EggAnt>();
        public IEnumerable<BuilderAnt> BuilderAnts => FilterEntities<BuilderAnt>();
        public IEnumerable<Instances.WarriorAnt.WarriorAnt> WarriorAnts => FilterEntities<Instances.WarriorAnt.WarriorAnt>();
        public IEnumerable<QueenAnt> QueenAnts => FilterEntities<QueenAnt>();
        public IEnumerable<Food.Food> Foods => FilterEntities<Food.Food>();

        public IEnumerable<TEntityType> FilterEntities<TEntityType>() where TEntityType : Entity
        {
            List<TEntityType> snapshot = new List<TEntityType>();

            lock (registryLock)
            {
                if (entityIdsPerType.TryGetValue(typeof(TEntityType), out List<uint> ids))
                {
                    foreach (uint ID in ids)
                    {
                        snapshot.Add(entities[ID] as TEntityType);
                    }
                }
            }

            return snapshot;
        }
    }

}