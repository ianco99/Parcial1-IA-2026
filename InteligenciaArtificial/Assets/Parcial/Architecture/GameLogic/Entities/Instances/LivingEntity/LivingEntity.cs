using System.Collections.Generic;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Services;
using KuznickiAgents.Pathfinding;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Map;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.LivingEntity
{
	public class LivingEntity : Entity, ITickable
	{
		Pathfinding Pathfinding => ServiceProvider.Instance.GetService<Pathfinding>();
		Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		
		public int Health;
		public int Speed;
		public float MoveTime;

		public PathfindingAlgorithm PathfindingAlgorithm = PathfindingAlgorithm.AStar;

		private List<Tile> path;
		private int pathIndex;

		private float elapsed = 0;
		private Point offsetBuffer;
		
		protected LivingEntity(uint ID, Coordinate coordinate, int teamId, int health, int speed, float moveTime) : base(ID, coordinate, teamId)
		{
			Health = health;
			Speed = speed;
			MoveTime = moveTime;
		}

		public int TerritoryTeamID => Scene.GetClosestTeam(Coordinate.Origin);
		public bool IsInOwnTerritory => TerritoryTeamID == teamID;

		public bool HasPath => path != null && pathIndex < path.Count;

		public bool FindPath(Point destiny)
		{
			path = Pathfinding.FindPath(Coordinate.Origin, destiny, teamID, PathfindingAlgorithm);
			pathIndex = 0;

			return path != null;
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);

			elapsed += deltaTime;
		}

		public bool MoveAlongPath()
		{
			if (!HasPath)
				return false;

			if (elapsed < MoveTime)
				return false;

			elapsed = 0;

			offsetBuffer = new Point(path[pathIndex].GetCoordinate().x, path[pathIndex].GetCoordinate().y);
			Move(offsetBuffer - Coordinate.Origin);

			pathIndex++;

			if (pathIndex >= path.Count)
				path = null;

			return true;
		}
		
		public Point GetPathDestiny() => new Point(path[pathIndex].GetCoordinate().x, path[pathIndex].GetCoordinate().y);
	}
}