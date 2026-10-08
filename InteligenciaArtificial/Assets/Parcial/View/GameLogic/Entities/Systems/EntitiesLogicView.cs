using System;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;
using Parcial.View.Mapping;

namespace Parcial.View.GameLogic.Entities.Systems
{
    [ViewOf(typeof(EntitiesLogic))]
    internal sealed class EntitiesLogicView : IInitable ,ITickable, IDisposable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
        private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        public EntitiesLogicView()
        {
            EventBus.SubscribeOnlyMainThread<EntityMovedEvent>(OnEntityMoved);
            EventBus.SubscribeOnlyMainThread<FoodAmountChangedEvent>(OnFoodAmountChanged);
        }

        private void OnFoodAmountChanged(in FoodAmountChangedEvent foodAmountChangedEvent)
        {
            if (!EntityRegistryView.Contains(foodAmountChangedEvent.foodID))
                return;

            EntityRegistryView.GetAs<FoodView>(foodAmountChangedEvent.foodID).
                SetAmount(foodAmountChangedEvent.currentAmount);
        }


        private void OnEntityMoved(in EntityMovedEvent entityMovedEvent)
        {
            if (!EntityRegistry.Contains(entityMovedEvent.movedEntityId) ||
                !EntityRegistryView.Contains(entityMovedEvent.movedEntityId))
                return;

            EntityRegistryView.GetAs<EntityView>(entityMovedEvent.movedEntityId).
                Move(EntityRegistry.GetAs<Entity>(entityMovedEvent.movedEntityId).Coordinate);
        }
        public void Init()
        {
        }

        public void LateInit()
        {
        }

        public void Tick(float deltaTime)
        {
            foreach (LivingEntityView livingEntityView in EntityRegistryView.LivingEntities)
                livingEntityView.Tick(deltaTime);
        }

        public void Dispose()
        {
            EventBus.UnsubscribeOnlyMainThread<EntityMovedEvent>(OnEntityMoved);
            EventBus.UnsubscribeOnlyMainThread<FoodAmountChangedEvent>(OnFoodAmountChanged);
        }

    }
}