namespace KuznickiAgents.Pathfinding
{
	public interface IPathfindingNode
	{
		public bool IsBlocked();
		public (int x, int y) GetCoordinate();
		public (int width, int height) GetDimensions();
		public int GetCost();
	}
}