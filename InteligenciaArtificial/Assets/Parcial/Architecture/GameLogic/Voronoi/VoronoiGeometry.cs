namespace Parcial.Architecture.GameLogic.Voronoi
{
	// Polygon helpers shared by the plain and the weighted diagram.
	internal static class VoronoiGeometry
	{
		public static double Dot(double aX, double aY, double bX, double bY)
		{
			return aX * bX + aY * bY;
		}

		// Positive when b is to the left of a (counter-clockwise from it), negative to the right, zero when aligned.
		public static double Cross(double aX, double aY, double bX, double bY)
		{
			return aX * bY - aY * bX;
		}

		// Even-odd test: also valid for the non-convex polygons a collapse or a push leaves.
		public static bool Contains((double x, double y)[] polygon, double x, double y)
		{
			bool inside = false;

			for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
			{
				(double x, double y) a = polygon[i];
				(double x, double y) b = polygon[j];

				if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x)
					inside = !inside;
			}

			return inside;
		}

		// The tiles a polygon can reach, inside a map of width x height tiles.
		public static (int minX, int minY, int maxX, int maxY) TileBounds((double x, double y)[] polygon, int width, int height)
		{
			double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

			foreach ((double x, double y) point in polygon)
			{
				minX = System.Math.Min(minX, point.x);
				minY = System.Math.Min(minY, point.y);
				maxX = System.Math.Max(maxX, point.x);
				maxY = System.Math.Max(maxY, point.y);
			}

			return ClampToTiles(minX, minY, maxX, maxY, width, height);
		}

		public static (int minX, int minY, int maxX, int maxY) ClampToTiles(double minX, double minY, double maxX, double maxY,
			int width, int height)
		{
			return (System.Math.Max(0, (int)System.Math.Floor(minX)),
				System.Math.Max(0, (int)System.Math.Floor(minY)),
				System.Math.Min(width - 1, (int)System.Math.Ceiling(maxX)),
				System.Math.Min(height - 1, (int)System.Math.Ceiling(maxY)));
		}
	}
}
