using System;

namespace Parcial.Architecture.Math
{
	public readonly struct Point : IEquatable<Point>
	{
		public static Point Right => new Point(1, 0);
		public static Point Left => new Point(-1, 0);
		public static Point Up => new Point(0, 1);
		public static Point Down => new Point(0, -1);

		public readonly int x;
		public readonly int y;

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static bool operator ==(in Point left,in Point right)
		{
			return left.x == right.x && left.y == right.y;
		}

		public static bool operator !=(in Point left,in Point right) 
		{
			return !(left == right);
		}

		public static Point operator + (in Point thisPoint,in Point other)
		{
			return new Point(thisPoint.x + other.x, thisPoint.y + other.y);
		}

        public static Point operator -(in Point thisPoint, in Point other)
        {
            return new Point(thisPoint.x - other.x, thisPoint.y - other.y);
        }

        public override bool Equals(object obj)
		{
			return obj is Point point &&
			       x == point.x &&
			       y == point.y;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y);
		}

		public bool Equals(Point other)
		{
			return this == other;
		}

		public override string ToString()
		{
			return $"X: {x} - Y: {y}";
		}
	}
}