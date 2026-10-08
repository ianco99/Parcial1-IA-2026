using System;
using KuznickiAgents.Pathfinding;

namespace Parcial.Architecture.GameLogic.Map
{
	public struct Tile : IPathfindingNode, IEquatable<Tile>
	{
		public int x;
		public int y;
		public int TileTypeId;
		public int TileTeamId;
		public uint closestFoodID;
		public int closestTeamID;

		public Tile(int x, int y, int tileTypeId, int tileTeamId, uint closestFoodID = 0,
			int closestTeamID = Entities.Entity.UNASSIGNED_ENTITY_TEAM_ID)
		{
			this.closestTeamID = closestTeamID;
			this.x = x;
			this.y = y;
			this.TileTypeId = tileTypeId;
			this.TileTeamId = tileTeamId;
			this.closestFoodID = closestFoodID;
		}

		public bool IsBlocked()
		{
			return false;
		}

		public (int x, int y) GetCoordinate()
		{
			return (x, y);
		}

		public (int width, int height) GetDimensions()
		{
			return (1, 1);
		}

		public int GetCost()
		{
			return 1;
		}

		public bool Equals(Tile other)
		{
			return x == other.x && y == other.y;
		}

		public override bool Equals(object obj)
		{
			return obj is Tile other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (x * 397) ^ y;
		}
	}
}