namespace KuznickiAgents.Pathfinding
{
	public class PathfindingNode : IPathfindingNode
	{
		private (int x, int y) coordinate;
		private (int width, int height) dimensions = (1, 1);
		private int cost = 1;
		private bool isBlocked;

		public void SetCoordinate((int x, int y) coordinate)
		{
			this.coordinate = coordinate;
		}

		public (int x, int y) GetCoordinate()
		{
			return coordinate;
		}

		public void SetDimensions((int width, int height) dimensions)
		{
			this.dimensions = dimensions;
		}

		public (int width, int height) GetDimensions()
		{
			return dimensions;
		}

		public void SetCost(int cost)
		{
			this.cost = cost;
		}

		public int GetCost()
		{
			return cost;
		}

		public bool IsBlocked()
		{
			return isBlocked;
		}

		public void SetIsBlocked(bool isBlocked)
		{
			this.isBlocked = isBlocked;
		}
	}
}