using System;
using System.Collections.Generic;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Exceptions;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Map
{
	public sealed class Map
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
		private BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();

		private uint sizeX;
		private uint sizeY;

		private uint tileX;
		private uint tileY;

		private Tile[,] grid;

		public Tile[,] Grid() => grid;
		
		private Dictionary<string, List<(int x, int y)>> instances;

		private Dictionary<int, TileData> tileDatas;
		private Dictionary<int, string> tileHashToName;

		private Dictionary<int, int[,]> weightedTeams;

		private List<string> uniqueTileDefinitions;
		public IReadOnlyList<string> UniqueTileDefinitions => uniqueTileDefinitions;

		private string habitatTileDefinition;
		private string anthillTileDefinition;
		public string HabitatTileDefinition => habitatTileDefinition;

		private string habitatWallTileDefinition;
		public string HabitatWallTileDefinition => habitatWallTileDefinition;

		private string humanEntryTileDefinition;
		public string HumanEntryTileDefinition => humanEntryTileDefinition;

		private string humanExitTileDefinition;
		public string HumanExitTileDefinition => humanExitTileDefinition;

		public bool HasInstancesOf(string tileID) => instances.ContainsKey(tileID) && instances[tileID].Count > 0;
		public int GetInstanceAmountOf(string tileID) => instances[tileID].Count;

		public IEnumerable<string> GetTileDefinitionIDs => tileHashToName.Values;

		private string defaultDataID;
		private int defaultDataHash;

		public Map(uint sizeX, uint sizeY, uint tileX, uint tileY)
		{
			this.sizeX = sizeX;
			this.sizeY = sizeY;

			this.tileX = tileX;
			this.tileY = tileY;

			EventBus.Raise<GridCreatedEvent>(sizeX, sizeY, tileX, tileY);

			tileDatas = new Dictionary<int, TileData>();
			tileHashToName = new Dictionary<int, string>();
			instances = new Dictionary<string, List<(int x, int y)>>();
			weightedTeams = new Dictionary<int, int[,]>();
			uniqueTileDefinitions = new List<string>();


			foreach (string tileTypeID in BlueprintRegistry.BlueprintsOf(TableNames.TILE_TYPES_TABLE_NAME))
			{
				object tileData = new TileData();

				try
				{
					BlueprintBinder.Apply(ref tileData, TableNames.TILE_TYPES_TABLE_NAME, tileTypeID);
				}
				catch (DataMisalignedException exception)
				{
					throw new DataEntryException(
						$"Failed to read {TableNames.TILE_TYPES_TABLE_NAME} - {tileTypeID}\n({exception.Message})");
				}

				if (((TileData)tileData).isUnique)
				{
					uniqueTileDefinitions.Add(tileTypeID);
				}

				if (((TileData)tileData).isAnthill)
				{
					anthillTileDefinition = tileTypeID;
				}


				tileDatas.Add(tileTypeID.GetHashCode(), (TileData)tileData);
				tileHashToName.Add(tileTypeID.GetHashCode(), tileTypeID);
			}

			grid = new Tile[sizeX, sizeY];
			defaultDataHash = 0;
			defaultDataID = string.Empty;
			foreach (KeyValuePair<int, TileData> tileData in tileDatas)
			{
				if (tileData.Value.isDefault)
				{
					defaultDataHash = tileData.Key.GetHashCode();
					defaultDataID = tileHashToName[defaultDataHash];
					break;
				}
			}

			if (defaultDataHash == 0)
				throw new BrokenGameRuleException("Missing default tile definition in Blueprints.xlsx");

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					grid[x, y] = new Tile(x,y,defaultDataHash, 0);

					if (!instances.ContainsKey(defaultDataID))
						instances.Add(defaultDataID, new List<(int x, int y)>());

					instances[defaultDataID].Add((x, y));

					EventBus.Raise<TileCreatedEvent>(defaultDataHash, x, y, 0);
				}
			}
		}

		public void SwapTile((int x, int y) coordinate, string newTileId, int terrainTeamId)
		{
			if (!tileHashToName.ContainsValue(newTileId))
				throw new System.MissingFieldException($"{newTileId} is not a valid Tile key definition.");

			if (!instances.ContainsKey(newTileId))
				instances.Add(newTileId, new List<(int x, int y)>());

			Tile originalTileInCoordinate = grid[coordinate.x, coordinate.y];
			string originalTileId = tileHashToName[originalTileInCoordinate.TileTypeId];

			instances[originalTileId].Remove(coordinate);
			instances[newTileId].Add(coordinate);

			grid[coordinate.x, coordinate.y].TileTypeId = newTileId.GetHashCode();
			grid[coordinate.x, coordinate.y].TileTeamId = terrainTeamId;

			EventBus.Raise<TileModifiedEvent>(grid[coordinate.x, coordinate.y].TileTypeId,
				coordinate.x, coordinate.y, terrainTeamId);
		}

		public void SetClosestFood(uint[,] owners)
		{
			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					grid[x, y].closestFoodID = owners[x, y];
				}
			}
		}

		public uint GetClosestFood(Point point)
		{
			return grid[point.x, point.y].closestFoodID;
		}

		public void SetClosestTeam(uint[,] owners, IReadOnlyDictionary<uint, int> siteTeams)
		{
			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					grid[x, y].closestTeamID = siteTeams.TryGetValue(owners[x, y], out int teamID)
						? teamID
						: Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID;
				}
			}
		}

		public int GetClosestTeam(Point point)
		{
			return grid[point.x, point.y].closestTeamID;
		}

		public void SetWeightedTeam(int viewerTeamID, uint[,] owners, IReadOnlyDictionary<uint, int> siteTeams)
		{
			if (!weightedTeams.TryGetValue(viewerTeamID, out int[,] teams))
			{
				teams = new int[sizeX, sizeY];
				weightedTeams.Add(viewerTeamID, teams);
			}

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					teams[x, y] = siteTeams.TryGetValue(owners[x, y], out int teamID)
						? teamID
						: Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID;
				}
			}
		}

		public int GetWeightedTeam(Point point, int viewerTeamID)
		{
			return weightedTeams.TryGetValue(viewerTeamID, out int[,] teams)
				? teams[point.x, point.y]
				: Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID;
		}

		public (uint x, uint y) GetSize() => (sizeX, sizeY);

		public bool IsCoordinateInsideMap(Coordinate coordinate)
		{
			(uint x, uint y) mapSize = GetSize();

			foreach (Point point in coordinate.Points)
			{
				if (point.x >= mapSize.x || point.x < 0 || point.y >= mapSize.y || point.y < 0)
					return false;
			}

			return true;
		}

		public Point[] GetNeighbors(Point point)
		{
			List<Point> neighbors = new List<Point>(8);

			Point[] directions =
			{
				Point.Up, Point.Right, Point.Down, Point.Left,
				Point.Up + Point.Right, Point.Down + Point.Right, Point.Down + Point.Left, Point.Up + Point.Left
			};

			foreach (Point direction in directions)
			{
				Point neighbor = point + direction;

				if (neighbor.x >= 0 && neighbor.x < sizeX && neighbor.y >= 0 && neighbor.y < sizeY)
					neighbors.Add(neighbor);
			}

			return neighbors.ToArray();
		}

		public bool IsDefaultTile(Point point)
		{
			return grid[point.x, point.y].TileTypeId == defaultDataHash;
		}

		public bool IsAnthillTile(Point point)
		{
			return tileHashToName[grid[point.x, point.y].TileTypeId] == anthillTileDefinition;
		}

		public bool IsTileFromTeam(Point point, int teamID)
		{
			return grid[point.x, point.y].TileTeamId == teamID;
		}

		public const int OWN_ANTHILL_TRAVEL_COST = 1;
		public const int DEFAULT_TRAVEL_COST = 2;
		public const int ENEMY_ANTHILL_TRAVEL_COST = 4;

		public const int RIVAL_AREA_TRAVEL_COST_MULTIPLIER = 2;

		public int GetTravelCost(Point point, int teamID)
		{
			if (!IsAnthillTile(point))
				return DEFAULT_TRAVEL_COST;

			return IsTileFromTeam(point, teamID) ? OWN_ANTHILL_TRAVEL_COST : ENEMY_ANTHILL_TRAVEL_COST;
		}

		public int GetWeightedTravelCost(Point point, int teamID)
		{
			int cost = GetTravelCost(point, teamID);
			int areaTeamID = GetWeightedTeam(point, teamID);

			if (areaTeamID == Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID || areaTeamID == teamID)
				return cost;

			return cost * RIVAL_AREA_TRAVEL_COST_MULTIPLIER;
		}

		public string AnthillTileDefinition => anthillTileDefinition;
		public string DefaultTileDefinition => defaultDataID;

		public List<Point> GetAnthillTilesOf(int teamID)
		{
			List<Point> tiles = new List<Point>();

			if (!HasInstancesOf(anthillTileDefinition))
				return tiles;

			foreach ((int x, int y) anthillTile in instances[anthillTileDefinition])
			{
				Point anthillPoint = new Point(anthillTile.x, anthillTile.y);

				if (IsTileFromTeam(anthillPoint, teamID))
					tiles.Add(anthillPoint);
			}

			return tiles;
		}

		public bool IsClaimableBy(Point point, int teamID)
		{
			if (!IsCoordinateInsideMap(new Coordinate(point)) || !IsDefaultTile(point))
				return false;

			foreach (Point neighbor in GetNeighbors(point))
			{
				if (IsAnthillTile(neighbor) && IsTileFromTeam(neighbor, teamID))
					return true;
			}

			return false;
		}

		public bool TryGetClaimableTile(int teamID, Point from, out Point tile)
		{
			tile = default;

			if (!HasInstancesOf(anthillTileDefinition))
				return false;

			bool found = false;
			int closestDistance = int.MaxValue;

			foreach ((int x, int y) anthillTile in instances[anthillTileDefinition])
			{
				Point anthillPoint = new Point(anthillTile.x, anthillTile.y);

				if (!IsTileFromTeam(anthillPoint, teamID))
					continue;

				foreach (Point neighbor in GetNeighbors(anthillPoint))
				{
					if (!IsDefaultTile(neighbor))
						continue;

					int distanceX = neighbor.x - from.x;
					int distanceY = neighbor.y - from.y;
					int distance = distanceX * distanceX + distanceY * distanceY;

					if (distance < closestDistance)
					{
						closestDistance = distance;
						tile = neighbor;
						found = true;
					}
				}
			}

			return found;
		}

		public Point GetHumanEntryPoint()
		{
			return new Point(instances[humanEntryTileDefinition][0].x, instances[humanEntryTileDefinition][0].y);
		}

		public Point GetHumanExitPoint()
		{
			return new Point(instances[humanExitTileDefinition][0].x, instances[humanExitTileDefinition][0].y);
		}

		public Coordinate GetCoordinate()
		{
			return new Coordinate(new Point(0, 0), new Point(Convert.ToInt32(sizeX), Convert.ToInt32(sizeY)));
		}
	}
}