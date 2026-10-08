using System;
using System.Collections.Generic;
using System.Diagnostics;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Config;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Anthill;
using Parcial.Architecture.GameLogic.Controllers.Anthill.Events;
using Parcial.Architecture.GameLogic.Controllers.ChangePathfinding;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.Architecture.GameLogic.Controllers.Team;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Food;
using Parcial.Architecture.GameLogic.Entities.Instances.WarriorAnt;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Map;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.Architecture.GameLogic.Voronoi;
using Parcial.Architecture.Math;

namespace Parcial.Architecture
{
	public sealed class Scene : IService, IInitable, ITickable, IDisposable
	{
		public bool IsPersistance => false;

		private EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();
		private Pathfinding Pathfinding => ServiceProvider.Instance.GetService<Pathfinding>();

		private Map map;

		private SpawnAnthillControllerArchitecture spawnAnthillControllerArchitecture;
		private ModifyTerrainControllerArchitecture modifyTerrainControllerArchitecture;
		private ChangePathfindingAlgorithmControllerArchitecture changePathfindingAlgorithmControllerArchitecture;

		private Dictionary<int, SpawnTeamControllerArchitecture> spawnTeamControllerArchitectures;

		public void Init()
		{
			ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
			ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());
			ServiceProvider.Instance.AddService<EntitiesLogic>(new EntitiesLogic());
			ServiceProvider.Instance.AddService<Pathfinding>(new Pathfinding());
			ServiceProvider.Instance.AddService<VoronoiService>(new VoronoiService());
			ServiceProvider.Instance.AddService<WeightedVoronoiService>(new WeightedVoronoiService());
			EntitiesLogic.Init();
			Pathfinding.Init();
		}

		public void LateInit()
		{
			modifyTerrainControllerArchitecture = new ModifyTerrainControllerArchitecture();
			changePathfindingAlgorithmControllerArchitecture = new ChangePathfindingAlgorithmControllerArchitecture();
			spawnTeamControllerArchitectures = new Dictionary<int, SpawnTeamControllerArchitecture>();

			EventBus.Subscribe<ModifyTerrainRequestAcceptedEvent>(OnModifyTerrainRequestAccepted);
			EventBus.Subscribe<ModifyTerrainEXRequestAcceptedEvent>(OnModifyTerrainEXRequestAccepted);

			map = new Map(MapConfigReader.MapWidth, MapConfigReader.MapHeight,
				MapConfigReader.TileWidth, MapConfigReader.TileHeight);

			TeamsSetup();
            TeamsLateInit();
			Pathfinding.LateInit();
			EventBus.Raise<MapCreatedEvent>();

			EntitiesLogic.LateInit();
		}

		private void TeamsSetup()
		{
			int teamAmount = (int)MapConfigReader.AnthillAmount;

			for (int teamIndex = 0; teamIndex < teamAmount; teamIndex++)
			{
				spawnTeamControllerArchitectures.Add(teamIndex, new SpawnTeamControllerArchitecture(teamIndex));
				spawnTeamControllerArchitectures[teamIndex].Init();
			}
		}

		private void TeamsLateInit()
		{
			foreach (var item in spawnTeamControllerArchitectures)
			{
				item.Value.LateInit();
			}
		}

		public void Tick(float deltaTime)
		{
			EntitiesLogic.Tick(deltaTime);

			foreach (var item in spawnTeamControllerArchitectures)
			{
				item.Value.ClaimTileControllerArchitecture.ResolveClaims();
			}
		}

		public void Dispose()
		{
			EntitiesLogic.Dispose();
			EntityFactory.Dispose();
			modifyTerrainControllerArchitecture.Dispose();
			changePathfindingAlgorithmControllerArchitecture.Dispose();

			foreach (var item in spawnTeamControllerArchitectures)
			{
				item.Value.Dispose();
			}

			EventBus.UnSubscribe<ModifyTerrainRequestAcceptedEvent>(OnModifyTerrainRequestAccepted);
			EventBus.UnSubscribe<ModifyTerrainEXRequestAcceptedEvent>(OnModifyTerrainEXRequestAccepted);
		}

		private void OnModifyTerrainRequestAccepted(in ModifyTerrainRequestAcceptedEvent modifyTerrainRequestEvent)
		{
			for (int x = modifyTerrainRequestEvent.origin.x; x <= modifyTerrainRequestEvent.end.x; x++)
			{
				for (int y = modifyTerrainRequestEvent.origin.y; y <= modifyTerrainRequestEvent.end.y; y++)
				{
					map.SwapTile((x, y), modifyTerrainRequestEvent.newTileId, modifyTerrainRequestEvent.terrainTeamId);
				}
			}
		}

		private void OnModifyTerrainEXRequestAccepted(in ModifyTerrainEXRequestAcceptedEvent modifyTerrainRequestEvent)
		{
			List<int> emptyTiles = new List<int>(modifyTerrainRequestEvent.tileExceptions);

			int index = 0;

			Point center = new Point(modifyTerrainRequestEvent.origin.x + 12, modifyTerrainRequestEvent.origin.y + 12);


			for (int x = modifyTerrainRequestEvent.origin.x; x <= modifyTerrainRequestEvent.end.x; x++)
			{
				for (int y = modifyTerrainRequestEvent.origin.y; y <= modifyTerrainRequestEvent.end.y; y++)
				{
					if (!emptyTiles.Contains(index))
						map.SwapTile((x, y), modifyTerrainRequestEvent.newTileId,
							modifyTerrainRequestEvent.terrainTeamId);

					if (index == 12)
						center = new Point(x, y);

					index++;
				}
			}

			EventBus.Raise<AnthillSpawnedEvent>(modifyTerrainRequestEvent.origin, modifyTerrainRequestEvent.end, center,
				modifyTerrainRequestEvent.terrainTeamId);
		}

		public bool IsCoordinateInsideMap(Coordinate coordinate)
		{
			return map.IsCoordinateInsideMap(coordinate);
		}

		public bool CanPlaceArea(Point origin, Point end, int margin = 0)
		{
			return CanPlaceArea(new Coordinate(origin, end), margin);
		}

		public bool CanPlaceArea(Coordinate area, int margin = 0)
		{
			if (!IsDefaultArea(area))
				return false;

			Coordinate padded = new Coordinate(new Point(area.minX - margin, area.minY - margin),
				new Point(area.maxX + margin, area.maxY + margin));


			foreach (Entity entity in EntityRegistry.Entities)
			{
				if (entity.Coordinate.Overlaps(padded))
					return false;
			}

			return true;
		}


		public bool IsDefaultArea(Coordinate area)
		{
			if (!map.IsCoordinateInsideMap(area))
				return false;

			foreach (Point point in area.Points)
			{
				if (!map.IsDefaultTile(point))
					return false;
			}

			return true;
		}

		public string AnthillTileDefinition => map.AnthillTileDefinition;
		public string DefaultTileDefinition => map.DefaultTileDefinition;

		public List<Point> GetTeamAnthillTiles(int teamId) => map.GetAnthillTilesOf(teamId);

		public bool IsClaimableTile(Point point, int teamId) => map.IsClaimableBy(point, teamId);

		public bool TryGetClaimableTile(int teamId, Point from, out Point tile) =>
			map.TryGetClaimableTile(teamId, from, out tile);

		public int GetStoredFoodAmount(int teamId)
		{
			int amount = 0;

			foreach (Food food in EntityRegistry.Foods)
			{
				if (food.IsStored && food.teamID == teamId)
					amount += food.CurrentAmount;
			}

			return amount;
		}

		public bool TryConsumeStoredFood(int teamId, int amount)
		{
			if (GetStoredFoodAmount(teamId) < amount)
				return false;

			int consumed = 0;

			foreach (Food food in EntityRegistry.Foods)
			{
				if (!food.IsStored || food.teamID != teamId)
					continue;

				while (consumed < amount && food.CurrentAmount > 0)
				{
					food.GetCollected();
					consumed++;
				}

				if (consumed >= amount)
					break;
			}

			return true;
		}

		public int GetFoodFromTeam(int teamID)
		{
			SpawnTeamControllerArchitecture teamController = GetSpawnTeamControllerArchitecture(teamID);

			return teamController != null ? teamController.DepositedFood : 0;
		}

		public bool IsAnthillTile(Point point) => map.IsAnthillTile(point);

		public bool IsTeamAnthillTile(Point point, int teamId) =>
			map.IsAnthillTile(point) && map.IsTileFromTeam(point, teamId);

		public Point[] GetNeighboringTiles(Point point)
		{
			return map.GetNeighbors(point);
		}

		public (Point, uint) GetNeighboringFood(Point point)
		{
			Point[] points = map.GetNeighbors(point);
			
			for (int i = 0; i < points.Length; i++)
			{
				foreach (Food food in EntityRegistry.Foods)
				{
					if (!food.IsStored && food.Coordinate.Origin == points[i])
					{
						return (food.Coordinate.Origin, food.ID);
					}
				}
			}

			return default;
		}

		public bool TryGetFoodDeposit(Point[] tiles, int teamId, out Food deposit, out Point freeTile)
		{
			deposit = null;
			freeTile = default;

			foreach (Food food in EntityRegistry.Foods)
			{
				if (!food.IsStored || food.teamID != teamId || food.CurrentAmount >= Food.MAX_AMOUNT)
					continue;

				for (int i = 0; i < tiles.Length; i++)
				{
					if (food.Coordinate.Origin == tiles[i])
					{
						deposit = food;
						return true;
					}
				}
			}

			for (int i = 0; i < tiles.Length; i++)
			{
				Coordinate tile = new Coordinate(tiles[i]);
				bool taken = false;

				foreach (Entity entity in EntityRegistry.Entities)
				{
					if (entity is BuilderAnt || entity is WarriorAnt)
						continue;

					if (entity.Coordinate.Overlaps(tile))
					{
						taken = true;
						break;
					}
				}

				if (!taken)
				{
					freeTile = tiles[i];
					return true;
				}
			}

			return false;
		}

		public Point GetEmptyAnthillPoint(Coordinate coordinate, int teamId)
		{
			bool foundEmpty = false;
			IEnumerable<Entity> entities = EntityRegistry.Entities;

			for (int x = coordinate.Origin.x; x <= coordinate.End.x; x++)
			{
				for (int y = coordinate.Origin.y; y <= coordinate.End.y; y++)
				{
					Coordinate currentCoordinate = new Coordinate(x, y);

					if (map.IsAnthillTile(currentCoordinate.Origin) &&
					    map.IsTileFromTeam(currentCoordinate.Origin, teamId))
					{
						if (IsCoordinateEmpty(currentCoordinate, entities))
						{
							return currentCoordinate.Origin;
						}
					}
				}
			}

			return default(Point);
		}

		public Point[] GetEmptyAnthillPoints(Coordinate coordinate, int teamId)
		{
			List<Point> points = new List<Point>();

            bool foundEmpty = false;
            IEnumerable<Entity> entities = EntityRegistry.Entities;

            for (int x = coordinate.Origin.x; x <= coordinate.End.x; x++)
            {
                for (int y = coordinate.Origin.y; y <= coordinate.End.y; y++)
                {
                    Coordinate currentCoordinate = new Coordinate(x, y);

                    if (map.IsAnthillTile(currentCoordinate.Origin) &&
                        map.IsTileFromTeam(currentCoordinate.Origin, teamId))
                    {
                        if (IsCoordinateEmpty(currentCoordinate, entities))
                        {
							points.Add(currentCoordinate.Origin);
                        }
                    }
                }
            }

            return points.ToArray();
        }

		public bool IsCoordinateEmpty(Coordinate point)
		{
			return IsCoordinateEmpty(point, EntityRegistry.Entities);
		}

		private bool IsCoordinateEmpty(Coordinate point, IEnumerable<Entity> entities)
		{
			foreach (Entity entity in entities)
			{
				if (entity.Coordinate.Overlaps(point))
				{
					return false;
				}
			}

			return true;
		}

		public Tile[,] GetMapTiles() => map.Grid();

		public void SetTilesClosestFood(uint[,] owners) => map.SetClosestFood(owners);

		public uint GetClosestFood(Point point) => map.GetClosestFood(point);

		public void SetTilesClosestTeam(uint[,] owners, IReadOnlyDictionary<uint, int> siteTeams) =>
			map.SetClosestTeam(owners, siteTeams);

		public int GetClosestTeam(Point point) => map.GetClosestTeam(point);

		public void SetTilesWeightedTeam(int teamId, uint[,] owners, IReadOnlyDictionary<uint, int> siteTeams) =>
			map.SetWeightedTeam(teamId, owners, siteTeams);

		public int GetWeightedTeam(Point point, int teamId) => map.GetWeightedTeam(point, teamId);

		public int GetTileTravelCost(Point point, int teamId) => map.GetTravelCost(point, teamId);

		public int GetTileWeightedTravelCost(Point point, int teamId) => map.GetWeightedTravelCost(point, teamId);

		internal SpawnTeamControllerArchitecture GetSpawnTeamControllerArchitecture(int teamId)
		{
			if (spawnTeamControllerArchitectures.TryGetValue(teamId, out SpawnTeamControllerArchitecture valor))
				return valor;
			else
				return null;
		}


		public Coordinate MapCoordinate => map.GetCoordinate();
	}
}