
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Scheduling;
using ianco99.ToolBox.Services;
using Parcial.Architecture;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Food;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Voronoi;
using Parcial.Architecture.GameLogic.Voronoi.Events;
using Parcial.Architecture.Math;
using System;
using System.Collections.Concurrent;
using ZooArchitect.Architecture.Entities.Events;

public sealed class FoodLogic : IInitable, ITickable, IDisposable
{
    Scene Scene => ServiceProvider.Instance.GetService<Scene>();
    TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();
    EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    VoronoiService VoronoiService => ServiceProvider.Instance.GetService<VoronoiService>();

    Random r;

    int width;
    int height;
    int x;
    int y;

    Coordinate chosen;

    ConcurrentQueue<(bool added, uint id, Point tile)> pendingChanges;

    VoronoiDiagram diagram;
    public VoronoiDiagram Diagram => diagram;

    public void Dispose()
    {
        EventBus.UnSubscribe<EntityCreatedEvent<Food>>(OnFoodCreated);
        EventBus.UnSubscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        EventBus.UnSubscribe<QueenKilledEvent>(OnQueenKilled);
    }

    public void Tick(float deltaTime)
    {
        bool changed = false;

        while (pendingChanges.TryDequeue(out (bool added, uint id, Point tile) change))
        {
            if (change.added)
                changed |= diagram.AddNode(new VoronoiNode(change.id, change.tile)).Count > 0;
            else if (diagram.Areas.ContainsKey(change.id))
            {
                diagram.RemoveNode(change.id);
                changed = true;
            }
        }

        if (changed)
        {
            Scene.SetTilesClosestFood(diagram.TileOwners);
            EventBus.Raise<FoodVoronoiRecalculatedEvent>(diagram);
        }
    }

    private void OnFoodCreated(in EntityCreatedEvent<Food> foodCreatedEvent)
    {
        if (foodCreatedEvent.entityTeamID != Parcial.Architecture.GameLogic.Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID)
            return;

        pendingChanges.Enqueue((true,foodCreatedEvent.EntityCreatedId, foodCreatedEvent.Coordinate.Origin));
    }

    private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
    {
        pendingChanges.Enqueue((false, entityDestroyedEvent.entityID, default));
    }

    private void OnQueenKilled(in QueenKilledEvent queenKilledEvent)
    {
        foreach (Food storedFood in EntityRegistry.Foods)
        {
            if (!storedFood.IsStored || storedFood.teamID != queenKilledEvent.teamId)
                continue;

            Coordinate tile = storedFood.Coordinate;
            int amount = storedFood.CurrentAmount;

            EntityRegistry.UnRegister(storedFood);

            EventBus.Raise<SpawnRequestAcceptedEvent<Food>>(EntitiesLogic.GetFoodBlueprint(), tile,
                TableNames.ANTS_TABLE_NAME, Parcial.Architecture.GameLogic.Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID);

            foreach (Food food in EntityRegistry.Foods)
            {
                if (!food.IsStored && food.Coordinate.Origin == tile.Origin)
                {
                    food.SetAmount(amount);
                    break;
                }
            }
        }
    }

    private void SpawnFood()
    {

        width = Scene.MapCoordinate.Width;
        height = Scene.MapCoordinate.Height;

        x = r.Next(1, width-1);
        y =  r.Next(1, height-1);



        chosen = new Coordinate(x, y);

        if(Scene.IsCoordinateEmpty(chosen))
        {
            EventBus.Raise<SpawnRequestAcceptedEvent<Food>>(EntitiesLogic.GetFoodBlueprint(), chosen, TableNames.ANTS_TABLE_NAME, -1);
        }

        TaskScheduler.Schedule(SpawnFood, 2.0f);
    }

    public void Init()
    {
        r = new Random();
        pendingChanges = new ConcurrentQueue<(bool added, uint id, Point tile)>();

        EventBus.Subscribe<EntityCreatedEvent<Food>>(OnFoodCreated);
        EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        EventBus.Subscribe<QueenKilledEvent>(OnQueenKilled);
    }

    public void LateInit()
    {
        diagram = VoronoiService.CreateDiagram(Scene.GetMapTiles().GetLength(0), Scene.GetMapTiles().GetLength(1));

        TaskScheduler.Schedule(SpawnFood, 2.0f);
    }
}
