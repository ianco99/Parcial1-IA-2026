using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Controllers.BuilderAnt;
using Parcial.Architecture.GameLogic.Controllers.ChangePathfinding.Events;
using Parcial.Architecture.GameLogic.Controllers.WarriorAnt;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Systems
{
    public sealed class EntitiesLogic : IService, IInitable, ITickable, IDisposable
    {
        public bool IsPersistance => false;

        private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();

        private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        FoodLogic foodLogic;
        TerritoryLogic territoryLogic;
        private BuilderAntsControllerArchitecture builderAntsControllerArchitecture;
        private WarriorAntsControllerArchitecture warriorAntsControllerArchitecture;


        private readonly List<Entity> tickBuffer;

        public EntitiesLogic()
        {
            tickBuffer = new List<Entity>();
            foodLogic = new FoodLogic();
            territoryLogic = new TerritoryLogic();
            builderAntsControllerArchitecture = new BuilderAntsControllerArchitecture();
            warriorAntsControllerArchitecture = new WarriorAntsControllerArchitecture();
            EventBus.Subscribe<ChangePathfindingAlgorithmRequestAcceptedEvent>(OnChangePathfindingAlgorithmRequestAccepted);
        }

        private void OnChangePathfindingAlgorithmRequestAccepted(in ChangePathfindingAlgorithmRequestAcceptedEvent callback)
        {
            EntityRegistry.GetAs<LivingEntity.LivingEntity>(callback.entityId).PathfindingAlgorithm = callback.algorithm;
        }


        public void Tick(float deltaTime)
        {
            foodLogic.Tick(deltaTime);
            territoryLogic.Tick(deltaTime);

            //tickBuffer.Clear();
            //tickBuffer.AddRange(EntityRegistry.Entities);

            foreach (QueenAnt queenAnt in EntityRegistry.QueenAnts)
                queenAnt.Tick(deltaTime);

            builderAntsControllerArchitecture.Tick(deltaTime);
            warriorAntsControllerArchitecture.Tick(deltaTime);
        }

        public void Dispose()
        {
            foodLogic.Dispose();
            territoryLogic.Dispose();
            builderAntsControllerArchitecture.Dispose();
            warriorAntsControllerArchitecture.Dispose();
            EventBus.UnSubscribe<ChangePathfindingAlgorithmRequestAcceptedEvent>(OnChangePathfindingAlgorithmRequestAccepted);
        }
        
        public string GetQueenAntBlueprint() => BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME)[0];
            


        public string GetEggAntBlueprint()
        {
            return BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME)[1];
        }

        public string GetBuilderAntBlueprint()
        {
            return BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME)[2];
        }
        public string GetWarriorAntBlueprint() => BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME)[3];
        public string GetFoodBlueprint() => BlueprintRegistry.BlueprintsOf(TableNames.ANTS_TABLE_NAME)[4];

        public void Init()
        {
            foodLogic.Init();
            territoryLogic.Init();
        }

        public void LateInit()
        {
            foodLogic.LateInit();
            territoryLogic.LateInit();
        }
    }
}