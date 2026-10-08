using System.Collections.Generic;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Services;
using KuznickiAgents.Pathfinding;
using Parcial.Architecture.Config;
using Parcial.Architecture.GameLogic.Map;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Systems
{
	public class Pathfinding : IService, IInitable
	{
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();
		public bool IsPersistance => false;

		private Dictionary<(int teamId, PathfindingAlgorithm algorithm), Pathfinder<Tile>> pathfinders;

		private ICollection<Tile> tiles;

		public List<Tile> FindPath(Point startPoint, Point endPoint, int teamId,
			PathfindingAlgorithm algorithm = PathfindingAlgorithm.AStar)
		{
			return pathfinders[(teamId, algorithm)].FindPath(Scene.GetMapTiles()[startPoint.x, startPoint.y],
				Scene.GetMapTiles()[endPoint.x, endPoint.y], tiles);
		}

		public void Init()
		{

		}

		public void LateInit()
		{
			List<Tile> tileArray = new List<Tile>();

			foreach (var VARIABLE in Scene.GetMapTiles())
			{
				tileArray.Add(VARIABLE);
			}

			tiles = tileArray;

			pathfinders = new Dictionary<(int teamId, PathfindingAlgorithm algorithm), Pathfinder<Tile>>();

			int teamAmount = (int)MapConfigReader.AnthillAmount;

			for (int teamIndex = 0; teamIndex < teamAmount; teamIndex++)
			{
				pathfinders.Add((teamIndex, PathfindingAlgorithm.AStar), new TeamAStarPathfinder(tiles, teamIndex));
				pathfinders.Add((teamIndex, PathfindingAlgorithm.Dijkstra), new TeamDijkstraPathfinder(tiles, teamIndex));
				pathfinders.Add((teamIndex, PathfindingAlgorithm.BreadthFirst), new BreadthFirstPathfinder<Tile>(tiles));
				pathfinders.Add((teamIndex, PathfindingAlgorithm.DepthFirst), new DepthFirstPathfinder<Tile>(tiles));
				pathfinders.Add((teamIndex, PathfindingAlgorithm.ThetaStar), new TeamThetaStarPathfinder(tiles, teamIndex));
			}
		}

		private sealed class TeamAStarPathfinder : AStarPathfinder<Tile>
		{
			Scene Scene => ServiceProvider.Instance.GetService<Scene>();

			private readonly int teamId;

			public TeamAStarPathfinder(ICollection<Tile> nodes, int teamId) : base(nodes)
			{
				this.teamId = teamId;
			}

			protected override int MoveToNeighborCost(Tile A, Tile B)
			{
				return Distance(A, B) * Scene.GetTileWeightedTravelCost(new Point(B.x, B.y), teamId);
			}
		}

		private sealed class TeamThetaStarPathfinder : ThetaStarPathfinder<Tile>
		{
			Scene Scene => ServiceProvider.Instance.GetService<Scene>();

			private readonly int teamId;

			public TeamThetaStarPathfinder(ICollection<Tile> nodes, int teamId) : base(nodes)
			{
				this.teamId = teamId;
			}

			protected override int MoveToNeighborCost(Tile A, Tile B)
			{
				return Distance(A, B) * Scene.GetTileWeightedTravelCost(new Point(B.x, B.y), teamId);
			}

			protected override bool CanOptimizeThrough(Tile node)
			{
				return Scene.IsTeamAnthillTile(new Point(node.x, node.y), teamId);
			}
		}

		private sealed class TeamDijkstraPathfinder: DijstraPathfinder<Tile>
		{
			Scene Scene => ServiceProvider.Instance.GetService<Scene>();

			private readonly int teamId;

			public TeamDijkstraPathfinder(ICollection<Tile> nodes, int teamId) : base(nodes)
			{
				this.teamId = teamId;
			}

			protected override int MoveToNeighborCost(Tile A, Tile B)
			{
				return Scene.GetTileWeightedTravelCost(new Point(B.x, B.y), teamId);
			}
		}
	}
}
