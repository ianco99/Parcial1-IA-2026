using System;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Instances.WarriorAnt;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

public class SpawnWarriorAntControllerArchitecture : IInitable, IDisposable
{
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    Scene Scene => ServiceProvider.Instance.GetService<Scene>();
    EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();

    int teamID;

    public SpawnWarriorAntControllerArchitecture(int teamID)
    {
        this.teamID = teamID;

        EventBus.Subscribe<SpawnRequestEvent<WarriorAnt>>(OnBuilderAntSpawned);
    }

    private void OnBuilderAntSpawned(in SpawnRequestEvent<WarriorAnt> callback)
    {
        if (callback.teamId != teamID) return;


        EventBus.Raise<SpawnRequestAcceptedEvent<WarriorAnt>>(callback.blueprintToSpawn, callback.coordinateToSpawn,
                TableNames.ANTS_TABLE_NAME, callback.teamId);
    }

    public void Dispose()
    {
        EventBus.UnSubscribe<SpawnRequestEvent<WarriorAnt>>(OnBuilderAntSpawned);
    }

    public void Init()
    {

    }

    public void LateInit()
    {
        Coordinate anthill = Scene.GetSpawnTeamControllerArchitecture(teamID)
            .SpawnAnthillControllerArchitecture.MyAnthill;


        Point[] points = Scene.GetEmptyAnthillPoints(anthill, teamID);

        for (int i = 0; i < 3; i++)
        {
            EventBus.Raise<SpawnRequestEvent<WarriorAnt>>(EntitiesLogic.GetWarriorAntBlueprint(), new Coordinate(points[i]), teamID);
        }
    }
}