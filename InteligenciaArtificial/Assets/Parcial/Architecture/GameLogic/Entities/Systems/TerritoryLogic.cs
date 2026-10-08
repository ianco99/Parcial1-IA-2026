using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture;
using Parcial.Architecture.Config;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.Architecture.GameLogic.Voronoi;
using Parcial.Architecture.GameLogic.Voronoi.Events;
using Parcial.Architecture.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZooArchitect.Architecture.Entities.Events;

public sealed class TerritoryLogic : IInitable, ITickable, IDisposable
{
    Scene Scene => ServiceProvider.Instance.GetService<Scene>();
    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    VoronoiService VoronoiService => ServiceProvider.Instance.GetService<VoronoiService>();
    WeightedVoronoiService WeightedVoronoiService => ServiceProvider.Instance.GetService<WeightedVoronoiService>();
    MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();

    ConcurrentQueue<(bool added, uint id, Point tile, int teamID)> pendingChanges;

    Dictionary<uint, int> siteTeams;

    VoronoiDiagram diagram;
    public VoronoiDiagram Diagram => diagram;

    WeightedVoronoiDiagram[] weightedDiagrams;

    ConcurrentQueue<Point> modifiedTiles;

    public void Init()
    {
        pendingChanges = new ConcurrentQueue<(bool added, uint id, Point tile, int teamID)>();
        modifiedTiles = new ConcurrentQueue<Point>();
        siteTeams = new Dictionary<uint, int>();

        EventBus.Subscribe<EntityCreatedEvent<QueenAnt>>(OnQueenCreated);
        EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        EventBus.Subscribe<TileModifiedEvent>(OnTileModified);
    }

    public void LateInit()
    {
        int width = Scene.GetMapTiles().GetLength(0);
        int height = Scene.GetMapTiles().GetLength(1);

        diagram = VoronoiService.CreateDiagram(width, height);

        weightedDiagrams = new WeightedVoronoiDiagram[(int)MapConfigReader.AnthillAmount];

        for (int teamIndex = 0; teamIndex < weightedDiagrams.Length; teamIndex++)
        {
            int teamID = teamIndex;

            weightedDiagrams[teamIndex] = WeightedVoronoiService.CreateDiagram(width, height,
                point => Scene.GetTileTravelCost(point, teamID));
        }
    }

    public void Tick(float deltaTime)
    {
        bool changed = false;
        bool tilesSwapped = false;

        while (modifiedTiles.TryDequeue(out _))
        {
            tilesSwapped = true;
        }

        while (pendingChanges.TryDequeue(out (bool added, uint id, Point tile, int teamID) change))
        {
            if (change.added)
            {
                VoronoiNode node = new VoronoiNode(change.id, change.tile);

                if (diagram.AddNode(node).Count > 0)
                {
                    siteTeams[change.id] = change.teamID;
                    changed = true;

                    Parallel.For(0, weightedDiagrams.Length, teamID => { weightedDiagrams[teamID].AddNode(node); });
                }
            }
            else if (diagram.Areas.ContainsKey(change.id))
            {
                uint removedId = change.id;

                diagram.RemoveNode(removedId);
                siteTeams.Remove(removedId);
                changed = true;

                Parallel.For(0, weightedDiagrams.Length, teamID => { weightedDiagrams[teamID].RemoveNode(removedId); });
            }
        }

        if (changed)
        {
            Scene.SetTilesClosestTeam(diagram.TileOwners, siteTeams);
            EventBus.Raise<TerritoryVoronoiRecalculatedEvent>(diagram);
        }
        else if (tilesSwapped)
        {
            Parallel.For(0, weightedDiagrams.Length, teamID => { weightedDiagrams[teamID].Rebalance(); });
        }

        if (changed || tilesSwapped)
        {
            for (int teamID = 0; teamID < weightedDiagrams.Length; teamID++)
            {
                Scene.SetTilesWeightedTeam(teamID, weightedDiagrams[teamID].TileOwners, siteTeams);
                EventBus.Raise<WeightedVoronoiRecalculatedEvent>(teamID, weightedDiagrams[teamID]);
            }
        }
    }

    private void OnQueenCreated(in EntityCreatedEvent<QueenAnt> queenCreatedEvent)
    {
        pendingChanges.Enqueue((true, queenCreatedEvent.EntityCreatedId, queenCreatedEvent.Coordinate.Origin,
            queenCreatedEvent.entityTeamID));
    }

    private void OnEntityDestroyed(in EntityDestroyedEvent entityDestroyedEvent)
    {
        pendingChanges.Enqueue((false, entityDestroyedEvent.entityID, default, Entity.UNASSIGNED_ENTITY_TEAM_ID));
    }

    private void OnTileModified(in TileModifiedEvent tileModifiedEvent)
    {
        modifiedTiles.Enqueue(new Point(tileModifiedEvent.xCoord, tileModifiedEvent.yCoord));
    }

    public void Dispose()
    {
        EventBus.UnSubscribe<EntityCreatedEvent<QueenAnt>>(OnQueenCreated);
        EventBus.UnSubscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        EventBus.UnSubscribe<TileModifiedEvent>(OnTileModified);
    }
}
