using ianco99.ToolBox.DataFlow;

namespace Parcial.Architecture.GameLogic.Map
{
	public sealed class Grid : IInitable
	{
		private int sizeX;
		private int sizeY;
		
		private int tileSizeX;
		private int tileSizeY;
		
		private Tile[,] grid;
		
		public Grid(int sizeX, int sizeY, int tileSizeX, int tileSizeY)
		{
			this.sizeX = sizeX;
			this.sizeY = sizeY;

			GenerateGrid();
		}

		public void Init()
		{
			
		}

		public void LateInit()
		{
			
		}

		private void GenerateGrid()
		{
			grid = new Tile[sizeX, sizeY];
			
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					
				}
			}
		}
	}
}