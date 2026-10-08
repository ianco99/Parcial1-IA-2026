using System;
using System.Collections.Generic;

namespace Parcial.Architecture.Math
{
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        public readonly int minX;
        public readonly int maxX;
        public readonly int minY;
        public readonly int maxY;

        public bool IsSingleCoordinate => minX == maxX && minY == maxY;
        public Point Origin => new Point(minX, minY);
        public Point End => new Point(maxX, maxY);

        public int Width => (maxX - minX) + 1;
        public int Height => (maxY - minY) + 1;

        public Coordinate(Point a, Point b)
        {
            minX = a.x < b.x ? a.x : b.x;
            maxX = a.x > b.x ? a.x : b.x;
            minY = a.y < b.y ? a.y : b.y;
            maxY = a.y > b.y ? a.y : b.y;
        }

        public Coordinate(int x, int y) : this(new Point(x, y), new Point(x, y)) { }

        public Coordinate(Point point) : this(point, point) { }


        public IEnumerable<Point> Points
        {
            get
            {
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }

        public IEnumerable<Point> Inner
        {
            get
            {
                if (Width <= 2 || Height <= 2)
                    yield break;

                for (int x = minX + 1; x < maxX; x++)
                {
                    for (int y = minY + 1; y < maxY; y++)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }

        public IEnumerable<Point> Perimeter
        {
            get
            {
                if (IsSingleCoordinate)
                {
                    yield return Origin;
                    yield break;
                }

                for (int x = minX; x <= maxX; x++)
                {
                    yield return new Point(x, minY);
                }

                for (int y = minY + 1; y < maxY; y++)
                {
                    yield return new Point(maxX, y);
                }

                if (maxY > minY)
                {
                    for (int x = maxX; x >= minX; x--)
                    {
                        yield return new Point(x, maxY);
                    }
                }

                if (maxX > minX)
                {
                    for (int y = maxY - 1; y > minY; y--)
                    {
                        yield return new Point(minX, y);
                    }
                }
            }
        }

        public Coordinate Move(in Point offset)
        {
            return new Coordinate(new Point(minX + offset.x, minY + offset.y),
                                  new Point(maxX + offset.x, maxY + offset.y));
        }

        public bool Overlaps(in Coordinate other)
        {
            return End.x >= other.Origin.x &&
                   Origin.x <= other.End.x &&
                   End.y >= other.Origin.y &&
                   Origin.y <= other.End.y;
        }

        public bool IsInPerimeter(in Coordinate other)
        {
            if (!Overlaps(other))
                return false;

            return End.x == other.Origin.x ||
                   Origin.x == other.End.x ||
                   End.y == other.Origin.y ||
                   Origin.y == other.End.y;
        }

        public bool IsInInner(in Coordinate other)
        {
            return Origin.x < other.Origin.x &&
                   End.x > other.End.x &&
                   Origin.y < other.Origin.y &&
                   End.y > other.End.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Coordinate coordinate &&
                   minX == coordinate.minX &&
                   maxX == coordinate.maxX &&
                   minY == coordinate.minY &&
                   maxY == coordinate.maxY;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(minX, maxX, minY, maxY);
        }

        public bool Equals(Coordinate other)
        {
            return this == other;
        }

        public static bool operator ==(in Coordinate left, in Coordinate right)
        {
            return left.minX == right.minX && left.minY == right.minY &&
                left.maxX == right.maxX && left.maxY == right.maxY;
        }

        public static bool operator !=(in Coordinate left, in Coordinate right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Coordinate from {Origin} to {End}";
        }
    }
}