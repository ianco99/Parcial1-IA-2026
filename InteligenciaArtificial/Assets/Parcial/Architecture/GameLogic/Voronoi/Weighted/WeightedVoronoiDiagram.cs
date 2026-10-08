using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class WeightedVoronoiDiagram
	{
		public const int MAX_ITERATIONS = 7;

		private const double BALANCE_TOLERANCE = 0.02;

		// Of the way to the heaviest area's node, how far a vertex goes in the first round. Each round after
		// that goes half as far as the one before.
		private const double FIRST_STEP_FRACTION = 0.5;

		// The first step, per unit the worst area is off the average: 10% off starts at 5% of the way.
		private const double STEP_PER_DEVIATION = 0.5;

		private const double EPSILON = 1e-6;
		private const double BORDER_SNAP = 1e-3;

		private readonly int width;
		private readonly int height;
		private readonly Func<Point, int> tileWeight;

		private readonly VoronoiDiagram baseDiagram;
		private readonly Dictionary<VoronoiVertex, (double x, double y)> offsets;
		private readonly Dictionary<uint, WeightedVoronoiArea> areas;
		private readonly uint[,] tileOwners;
		private readonly int[,] weights;

		private VoronoiVertex[] vertices;

		public IReadOnlyDictionary<uint, WeightedVoronoiArea> Areas => areas;

		public uint[,] TileOwners => tileOwners;

		public double AverageWeight { get; private set; }

		public int LastIterations { get; private set; }

		internal WeightedVoronoiDiagram(int width, int height, Func<Point, int> tileWeight)
		{
			this.width = width;
			this.height = height;
			this.tileWeight = tileWeight;

			baseDiagram = new VoronoiDiagram(width, height);
			offsets = new Dictionary<VoronoiVertex, (double x, double y)>();
			areas = new Dictionary<uint, WeightedVoronoiArea>();
			tileOwners = new uint[width, height];
			weights = new int[width, height];
			vertices = new VoronoiVertex[0];
		}

		public IReadOnlyCollection<uint> AddNode(VoronoiNode node)
		{
			if (baseDiagram.IsOutsideMap(node))
				throw new ArgumentOutOfRangeException(nameof(node), $"Voronoi node {node.id} is outside the map");

			bool wasExact = baseDiagram.IsExact;
			IReadOnlyCollection<uint> changed = baseDiagram.AddNode(node);

			if (changed.Count > 0 || !wasExact)
				Rebalance();

			return changed;
		}

		public IReadOnlyCollection<uint> RemoveNode(uint nodeId)
		{
			if (!areas.ContainsKey(nodeId))
				return Array.Empty<uint>();

			IReadOnlyCollection<uint> changed = baseDiagram.RemoveNode(nodeId);
			Rebalance();
			return changed;
		}

		public void Rebalance()
		{
			Sync();
			ReadWeights();

			Rasterize();
			Weigh();

			// The first step is as big as the areas are off, so a diagram that is almost level barely moves.
			double step = System.Math.Min(FIRST_STEP_FRACTION, WorstDeviation() * STEP_PER_DEVIATION);
			LastIterations = 0;

			while (LastIterations < MAX_ITERATIONS && !IsBalanced() && PushVertices(step))
			{
				// Binary search: half the step every round. A vertex that went too far comes back by itself,
				// because the area it moved into is no longer the heaviest one around it.
				step *= 0.5;
				LastIterations++;

				Rasterize();
				Weigh();
			}
		}

		/// <summary>
		/// Update base diagram
		/// </summary>
		private void Sync()
		{
			RemoveDeadAreas();
			HashSet<VoronoiVertex> alive = MirrorBaseAreas();
			LinkNeighbors();
			RemoveStaleOffsets(alive);

			vertices = new VoronoiVertex[alive.Count];
			alive.CopyTo(vertices);

			RefreshPolygons();
			ResetTangledAreas();
		}

		private void RemoveDeadAreas()
		{
			List<uint> removed = new List<uint>();

			foreach (uint id in areas.Keys)
			{
				if (!baseDiagram.Areas.ContainsKey(id))
					removed.Add(id);
			}

			foreach (uint id in removed)
			{
				areas.Remove(id);
			}
		}

		private HashSet<VoronoiVertex> MirrorBaseAreas()
		{
			HashSet<VoronoiVertex> alive = new HashSet<VoronoiVertex>();

			foreach (KeyValuePair<uint, VoronoiArea> pair in baseDiagram.Areas)
			{
				if (!areas.TryGetValue(pair.Key, out WeightedVoronoiArea area))
				{
					area = new WeightedVoronoiArea(pair.Value.Node);
					areas.Add(pair.Key, area);
				}

				area.baseArea = pair.Value;

				foreach (VoronoiVertex vertex in pair.Value.vertices)
				{
					alive.Add(vertex);
				}
			}

			return alive;
		}

		private void LinkNeighbors()
		{
			foreach (WeightedVoronoiArea area in areas.Values)
			{
				area.neighbors.Clear();

				foreach (VoronoiArea neighbor in area.baseArea.neighbors)
				{
					if (areas.TryGetValue(neighbor.Node.id, out WeightedVoronoiArea weightedNeighbor))
						area.neighbors.Add(weightedNeighbor);
				}
			}
		}

		private void RemoveStaleOffsets(HashSet<VoronoiVertex> alive)
		{
			List<VoronoiVertex> stale = new List<VoronoiVertex>();

			foreach (VoronoiVertex vertex in offsets.Keys)
			{
				if (!alive.Contains(vertex))
					stale.Add(vertex);
			}

			foreach (VoronoiVertex vertex in stale)
			{
				offsets.Remove(vertex);
			}
		}

		/// <summary>
		/// In case of polygon displacement resulting in not a convex polygon (tangled)
		/// </summary>
		private void ResetTangledAreas()
		{
			bool reset;

			do
			{
				reset = false;

				foreach (WeightedVoronoiArea area in areas.Values)
				{
					if (IsValid(area.polygon, area.Node) || !IsValid(area.baseArea.polygon, area.Node))
						continue;

					foreach (VoronoiVertex vertex in area.baseArea.vertices)
					{
						reset |= offsets.Remove(vertex);
					}
				}

				if (reset)
					RefreshPolygons();
			} while (reset);
		}

		private void ReadWeights()
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					weights[x, y] = tileWeight(new Point(x, y));
				}
			}
		}

		private void RefreshPolygons()
		{
			foreach (WeightedVoronoiArea area in areas.Values)
			{
				List<VoronoiVertex> baseVertices = area.baseArea.vertices;

				if (area.polygon.Length != baseVertices.Count)
					area.polygon = new (double x, double y)[baseVertices.Count];

				for (int i = 0; i < baseVertices.Count; i++)
				{
					area.polygon[i] = PositionOf(baseVertices[i]);
				}
			}
		}

		private (double x, double y) PositionOf(VoronoiVertex vertex)
		{
			return offsets.TryGetValue(vertex, out (double x, double y) offset)
				? (vertex.x + offset.x, vertex.y + offset.y)
				: (vertex.x, vertex.y);
		}

		private void Rasterize()
		{
			Array.Copy(baseDiagram.TileOwners, tileOwners, tileOwners.Length);

			foreach (WeightedVoronoiArea area in areas.Values)
			{
				(int minX, int minY, int maxX, int maxY) bounds = VoronoiGeometry.TileBounds(area.polygon, width, height);

				for (int x = bounds.minX; x <= bounds.maxX; x++)
				{
					for (int y = bounds.minY; y <= bounds.maxY; y++)
					{
						if (area.Contains(x + 0.5, y + 0.5))
							tileOwners[x, y] = area.Node.id;
					}
				}
			}

			foreach (WeightedVoronoiArea area in areas.Values)
			{
				tileOwners[area.Node.tile.x, area.Node.tile.y] = area.Node.id;
			}
		}

		private void Weigh()
		{
			foreach (WeightedVoronoiArea area in areas.Values)
			{
				area.Weight = 0;
				area.TileCount = 0;
			}

			long total = 0;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (!areas.TryGetValue(tileOwners[x, y], out WeightedVoronoiArea area))
						continue;

					area.Weight += weights[x, y];
					area.TileCount++;
					total += weights[x, y];
				}
			}

			AverageWeight = areas.Count > 0 ? (double)total / areas.Count : 0.0;

			foreach (WeightedVoronoiArea area in areas.Values)
			{
				area.DeviationFromAverage = area.Weight - AverageWeight;
			}
		}

		private bool IsBalanced()
		{
			return WorstDeviation() <= BALANCE_TOLERANCE;
		}

		// How far the area that is furthest from the average is, as a fraction of that average.
		private double WorstDeviation()
		{
			double worst = 0.0;

			foreach (WeightedVoronoiArea area in areas.Values)
			{
				worst = System.Math.Max(worst, System.Math.Abs(area.DeviationFromAverage));
			}

			return AverageWeight > 0.0 ? worst / AverageWeight : 0.0;
		}

		private bool PushVertices(double step)
		{
			(double x, double y)[] moves = new (double x, double y)[vertices.Length];

			// Every vertex decides by itself: it only reads the areas and writes its own slot.
			Parallel.For(0, vertices.Length, i => { moves[i] = MoveOf(vertices[i], step); });

			Dictionary<VoronoiVertex, (double x, double y)> applied = ApplyMoves(moves);

			if (applied.Count == 0)
				return false;

			RevertTangledMoves(applied);
			return applied.Count > 0;
		}

		private Dictionary<VoronoiVertex, (double x, double y)> ApplyMoves((double x, double y)[] moves)
		{
			Dictionary<VoronoiVertex, (double x, double y)> applied = new Dictionary<VoronoiVertex, (double x, double y)>();

			for (int i = 0; i < moves.Length; i++)
			{
				if (moves[i].x == 0.0 && moves[i].y == 0.0)
					continue;

				applied.Add(vertices[i], moves[i]);
				Displace(vertices[i], moves[i].x, moves[i].y);
			}

			return applied;
		}

		private void RevertTangledMoves(Dictionary<VoronoiVertex, (double x, double y)> applied)
		{
			foreach (WeightedVoronoiArea area in areas.Values)
			{
				area.wasValid = IsValid(area.polygon, area.Node);
			}

			RefreshPolygons();

			bool reverted;

			do
			{
				reverted = false;

				foreach (WeightedVoronoiArea area in areas.Values)
				{
					if (!area.wasValid || IsValid(area.polygon, area.Node))
						continue;

					foreach (VoronoiVertex vertex in area.baseArea.vertices)
					{
						if (!applied.TryGetValue(vertex, out (double x, double y) move))
							continue;

						applied.Remove(vertex);
						Displace(vertex, -move.x, -move.y);
						reverted = true;
					}
				}

				if (reverted)
					RefreshPolygons();
			} while (reverted);
		}

		private void Displace(VoronoiVertex vertex, double x, double y)
		{
			offsets.TryGetValue(vertex, out (double x, double y) offset);
			offsets[vertex] = (offset.x + x, offset.y + y);
		}

		// Where a vertex goes this round: into the heaviest of the areas it joins, the given fraction of the
		// way to its node. Nothing is simulated: if that was too far, the next round brings it back.
		private (double x, double y) MoveOf(VoronoiVertex vertex, double step)
		{
			bool lockedX = IsOnVerticalBorder(vertex);
			bool lockedY = IsOnHorizontalBorder(vertex);

			if (vertex.areas.Count < 2 || (lockedX && lockedY))
				return default;

			WeightedVoronoiArea heaviest = FindHeaviest(vertex);

			// Only an area above the average has something to give.
			if (heaviest == null || heaviest.Weight <= AverageWeight)
				return default;

			int index = heaviest.baseArea.vertices.IndexOf(vertex);
			(double x, double y) current = heaviest.polygon[index];
			(double x, double y) target = (heaviest.Node.X, heaviest.Node.Y);

			// On the map border a vertex can only slide along it, towards the other end of the stretch of
			// border the heaviest area holds.
			if (lockedX || lockedY)
			{
				(double x, double y) previous = heaviest.polygon[(index + heaviest.polygon.Length - 1) % heaviest.polygon.Length];
				(double x, double y) next = heaviest.polygon[(index + 1) % heaviest.polygon.Length];

				if (IsOnSameBorder(next, current, lockedX))
					target = next;
				else if (IsOnSameBorder(previous, current, lockedX))
					target = previous;
				else
					return default;
			}

			return (lockedX ? 0.0 : (target.x - current.x) * step,
				lockedY ? 0.0 : (target.y - current.y) * step);
		}

		/// <summary>
		/// Finds heaviest among neighbors
		/// </summary>
		/// <param name="vertex"></param>
		private WeightedVoronoiArea FindHeaviest(VoronoiVertex vertex)
		{
			WeightedVoronoiArea heaviest = null;

			foreach (VoronoiArea incident in vertex.areas)
			{
				if (!areas.TryGetValue(incident.Node.id, out WeightedVoronoiArea area))
					continue;

				if (heaviest == null || IsHeavier(area, heaviest))
					heaviest = area;
			}

			return heaviest;
		}

		private static bool IsOnSameBorder((double x, double y) other, (double x, double y) current, bool vertical)
		{
			return vertical
				? System.Math.Abs(other.x - current.x) < BORDER_SNAP
				: System.Math.Abs(other.y - current.y) < BORDER_SNAP;
		}

		private static bool IsHeavier(WeightedVoronoiArea area, WeightedVoronoiArea other)
		{
			return area.Weight > other.Weight || (area.Weight == other.Weight && area.Node.id < other.Node.id);
		}

		private bool IsOnVerticalBorder(VoronoiVertex vertex)
		{
			return vertex.x < BORDER_SNAP || vertex.x > width - BORDER_SNAP;
		}

		private bool IsOnHorizontalBorder(VoronoiVertex vertex)
		{
			return vertex.y < BORDER_SNAP || vertex.y > height - BORDER_SNAP;
		}

		private static bool IsValid((double x, double y)[] polygon, VoronoiNode node)
		{
			double doubleArea = 0.0;

			for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
			{
				(double x, double y) a = polygon[i];
				(double x, double y) b = polygon[j];

				doubleArea += VoronoiGeometry.Cross(b.x, b.y, a.x, a.y);
			}

			return doubleArea > EPSILON && VoronoiGeometry.Contains(polygon, node.X, node.Y);
		}
	}
}
