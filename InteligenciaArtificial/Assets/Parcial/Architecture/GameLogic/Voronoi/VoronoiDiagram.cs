using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Voronoi
{
	public sealed class VoronoiDiagram
	{
		public const uint MAP_BORDER_ID = Entity.UNASSIGNED_ENTITY_ID;

		private const double EPSILON = 1e-6;
		private const double DEGENERATE_EDGE_LENGTH = 1e-4;
		private const double VERTEX_SNAP = 1e-3;
		private const int MIN_POLYGON_VERTICES = 3;

		private readonly int width;
		private readonly int height;

		private readonly List<VoronoiNode> nodes;
		private readonly Dictionary<uint, VoronoiArea> areas;
		private readonly List<VoronoiVertex> vertices;
		private readonly uint[,] tileOwners;

		private readonly List<VoronoiArea> collapsed;

		public bool IsExact => collapsed.Count == 0;

		public IReadOnlyDictionary<uint, VoronoiArea> Areas => areas;
		public IReadOnlyCollection<VoronoiVertex> Vertices => vertices;

		public uint[,] TileOwners => tileOwners;

		internal struct RawPoint
		{
			public double x;
			public double y;
			public uint tag;

			public RawPoint(double x, double y, uint tag)
			{
				this.x = x;
				this.y = y;
				this.tag = tag;
			}
		}

		internal VoronoiDiagram(int width, int height)
		{
			this.width = width;
			this.height = height;

			nodes = new List<VoronoiNode>();
			areas = new Dictionary<uint, VoronoiArea>();
			vertices = new List<VoronoiVertex>();
			tileOwners = new uint[width, height];
			collapsed = new List<VoronoiArea>();
		}

		internal bool IsOutsideMap(VoronoiNode node)
		{
			return node.tile.x < 0 || node.tile.x >= width || node.tile.y < 0 || node.tile.y >= height;
		}

		public IReadOnlyCollection<uint> AddNode(VoronoiNode node)
		{
			if (areas.ContainsKey(node.id))
				return Array.Empty<uint>();

			List<VoronoiArea> touched = Heal();

			if (IsOutsideMap(node))
				throw new ArgumentOutOfRangeException(nameof(node), $"Voronoi node {node.id} is outside the map");

			if (!areas.TryGetValue(tileOwners[node.tile.x, node.tile.y], out VoronoiArea owner))
			{
				nodes.Add(node);
				return RebuildAll();
			}

			if (owner.Node.tile == node.tile)
				return Array.Empty<uint>();

			nodes.Add(node);

			List<RawPoint> polygon = ClipNewPolygon(node, owner);
			List<VoronoiArea> clipped = CreateAreas(node, polygon);

			Relink(clipped);
			ClaimTiles(node);

			AddMissing(touched, clipped);
			return IdsOf(touched);
		}

		private List<RawPoint> ClipNewPolygon(VoronoiNode node, VoronoiArea owner)
		{
			List<RawPoint> polygon = MapRectangle();
			Queue<VoronoiArea> pending = new Queue<VoronoiArea>();
			HashSet<uint> visited = new HashSet<uint> { owner.Node.id };
			pending.Enqueue(owner);

			while (pending.Count > 0)
			{
				VoronoiArea candidate = pending.Dequeue();
				polygon = Clip(polygon, node, candidate.Node);

				if (!PolygonContainsTagAmongVertices(polygon, candidate.Node.id))
					continue;

				//Is neighbor to current polygon
				foreach (VoronoiArea neighbor in candidate.neighbors)
				{
					if (visited.Add(neighbor.Node.id))
						pending.Enqueue(neighbor);
				}

				foreach (VoronoiVertex vertex in candidate.vertices)
				{
					foreach (VoronoiArea incident in vertex.areas)
					{
						if (visited.Add(incident.Node.id))
							pending.Enqueue(incident);
					}
				}
			}

			return polygon;
		}

		private List<VoronoiArea> CreateAreas(VoronoiNode node, List<RawPoint> polygon)
		{
			VoronoiArea area = new VoronoiArea(node);
			areas.Add(node.id, area);

			SetPolygon(area, polygon);

			List<VoronoiArea> clipped = new List<VoronoiArea> { area };

			
			//Reclip all neighboring areas
			foreach (uint neighborId in GetUniqueNeighborTags(polygon))
			{
				VoronoiArea neighbor = areas[neighborId];

				SetPolygon(neighbor, Clip(neighbor.GetRawVertices(), neighbor.Node, node));
				clipped.Add(neighbor);
			}

			return clipped;
		}

		public IReadOnlyCollection<uint> RemoveNode(uint nodeId)
		{
			if (!areas.TryGetValue(nodeId, out VoronoiArea dead))
				return Array.Empty<uint>();

			nodes.RemoveAll(node => node.id == nodeId);

			if (nodes.Count < 2 || dead.vertices.Exists(vertex => vertex.IsOnMapBorder))
				return RebuildAll();

			List<VoronoiArea> affected = FindAreasAround(dead);

			if (!TryCollapse(dead, affected))
				return RebuildAll();

			areas.Remove(nodeId);
			Relink(affected);
			ReassignTiles(nodeId, affected);

			// The dead area can still be stretched from an earlier removal: Heal must not get it.
			collapsed.Remove(dead);
			AddMissing(collapsed, affected);

			return IdsOf(affected);
		}

		// Every area that shares a vertex with the given one.
		private static List<VoronoiArea> FindAreasAround(VoronoiArea area)
		{
			List<VoronoiArea> around = new List<VoronoiArea>();

			foreach (VoronoiVertex vertex in area.vertices)
			{
				foreach (VoronoiArea incident in vertex.areas)
				{
					if (incident != area && !around.Contains(incident))
						around.Add(incident);
				}
			}

			return around;
		}

		// Moves every vertex of the dead area onto its node, which stretches the areas around it over the
		// space it leaves. False, without touching any area, when that would flatten one of them.
		private bool TryCollapse(VoronoiArea dead, List<VoronoiArea> affected)
		{
			VoronoiVertex center = GetOrCreateVertex(dead.Node.X, dead.Node.Y);
			HashSet<VoronoiVertex> collapsing = new HashSet<VoronoiVertex>(dead.vertices);

			List<VoronoiVertex>[] newVertices = new List<VoronoiVertex>[affected.Count];
			List<uint>[] newTags = new List<uint>[affected.Count];

			for (int i = 0; i < affected.Count; i++)
			{
				newVertices[i] = new List<VoronoiVertex>(affected[i].vertices.Count);
				newTags[i] = new List<uint>(affected[i].edgeTags);

				foreach (VoronoiVertex vertex in affected[i].vertices)
				{
					newVertices[i].Add(collapsing.Contains(vertex) ? center : vertex);
				}

				RemoveRepeatedVertices(newVertices[i], newTags[i]);

				if (newVertices[i].Count < MIN_POLYGON_VERTICES || newTags[i].Contains(dead.Node.id))
					return false;
			}

			for (int i = 0; i < affected.Count; i++)
			{
				UpdateAreaGeometry(affected[i], newVertices[i], newTags[i]);
			}

			foreach (VoronoiVertex vertex in dead.vertices)
			{
				Release(vertex, dead);
			}

			return true;
		}

		private IReadOnlyCollection<uint> RebuildAll()
		{
			Rebuild();
			return new List<uint>(areas.Keys);
		}

		private List<VoronoiArea> Heal()
		{
			List<VoronoiArea> healed = new List<VoronoiArea>(collapsed);

			if (healed.Count == 0)
				return healed;

			collapsed.Clear();

			VoronoiNode[] snapshot = nodes.ToArray();
			List<RawPoint>[] polygons = new List<RawPoint>[healed.Count];

			Parallel.For(0, healed.Count, i => { polygons[i] = ClipArea(healed[i].Node, snapshot); });

			for (int i = 0; i < healed.Count; i++)
			{
				SetPolygon(healed[i], polygons[i]);
			}

			// The tiles followed the stretched shapes: now they follow the exact ones.
			Relink(healed);
			ShareOutTiles(healed);
			return healed;
		}

		private void Rebuild()
		{
			areas.Clear();
			vertices.Clear();
			collapsed.Clear();

			VoronoiNode[] snapshot = nodes.ToArray();
			List<RawPoint>[] polygons = new List<RawPoint>[snapshot.Length];

			Parallel.For(0, snapshot.Length, i => { polygons[i] = ClipArea(snapshot[i], snapshot); });

			for (int i = 0; i < snapshot.Length; i++)
			{
				areas.Add(snapshot[i].id, new VoronoiArea(snapshot[i]));
			}

			for (int i = 0; i < snapshot.Length; i++)
			{
				SetPolygon(areas[snapshot[i].id], polygons[i]);
			}

			Relink(areas.Values);

			Parallel.For(0, width, x =>
			{
				for (int y = 0; y < height; y++)
				{
					tileOwners[x, y] = NearestNode(snapshot, x + 0.5, y + 0.5);
				}
			});
		}

		private List<RawPoint> ClipArea(VoronoiNode node, IReadOnlyList<VoronoiNode> candidates)
		{
			List<RawPoint> polygon = MapRectangle();

			for (int i = 0; i < candidates.Count; i++)
			{
				if (candidates[i].id != node.id)
					polygon = Clip(polygon, node, candidates[i]);
			}

			return polygon;
		}

		private List<RawPoint> MapRectangle()
		{
			return new List<RawPoint>
			{
				new RawPoint(0.0, 0.0, MAP_BORDER_ID),
				new RawPoint(width, 0.0, MAP_BORDER_ID),
				new RawPoint(width, height, MAP_BORDER_ID),
				new RawPoint(0.0, height, MAP_BORDER_ID)
			};
		}

		/// <summary>
		/// Clips polygon through bisector between two nodes
		/// </summary>
		/// <param name="polygon">Polygon to clip</param>
		/// <param name="node">Node that defines polygon</param>
		/// <param name="other">Neighboring node that defines clipping</param>
		/// <returns></returns>
		private static List<RawPoint> Clip(List<RawPoint> polygon, VoronoiNode node, VoronoiNode other)
		{
			double normalX = other.X - node.X;
			double normalY = other.Y - node.Y;
			double middleX = (other.X + node.X) * 0.5;
			double middleY = (other.Y + node.Y) * 0.5;

			List<RawPoint> result = new List<RawPoint>();

			for (int i = 0; i < polygon.Count; i++)
			{
				RawPoint current = polygon[i];
				RawPoint next = polygon[(i + 1) % polygon.Count];

				double currentSide = VoronoiGeometry.Dot(current.x - middleX, current.y - middleY, normalX, normalY);
				double nextSide = VoronoiGeometry.Dot(next.x - middleX, next.y - middleY, normalX, normalY);

				bool currentInside = currentSide <= EPSILON;
				bool nextInside = nextSide <= EPSILON;

				if (currentInside)
					result.Add(current);

				//Found point crossing bisector
				if (currentInside != nextInside)
				{
					
					double t = currentSide / (currentSide - nextSide);

					result.Add(new RawPoint(current.x + (next.x - current.x) * t,
						current.y + (next.y - current.y) * t,
						currentInside ? other.id : current.tag));
				}
			}

			RemoveDegenerateEdges(result);
			return result;
		}

		/// <summary>
		/// Remove
		/// </summary>
		/// <param name="polygon"></param>
		private static void RemoveDegenerateEdges(List<RawPoint> polygon)
		{
			for (int i = polygon.Count - 1; i >= 0 && polygon.Count > 1; i--)
			{
				RawPoint current = polygon[i];
				RawPoint next = polygon[(i + 1) % polygon.Count];

				double x = next.x - current.x;
				double y = next.y - current.y;

				if (x * x + y * y < DEGENERATE_EDGE_LENGTH * DEGENERATE_EDGE_LENGTH)
					polygon.RemoveAt(i);
			}
		}

		private static void RemoveRepeatedVertices(List<VoronoiVertex> polygon, List<uint> tags)
		{
			for (int i = polygon.Count - 1; i >= 0 && polygon.Count > 1; i--)
			{
				if (polygon[i] == polygon[(i + 1) % polygon.Count])
				{
					polygon.RemoveAt(i);
					tags.RemoveAt(i);
				}
			}
		}

		private static bool PolygonContainsTagAmongVertices(List<RawPoint> polygon, uint tag)
		{
			for (int i = 0; i < polygon.Count; i++)
			{
				if (polygon[i].tag == tag)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns all unique neighbors from all polygon vertices
		/// </summary>
		/// <param name="polygon"></param>
		/// <returns></returns>
		private static List<uint> GetUniqueNeighborTags(List<RawPoint> polygon)
		{
			List<uint> tags = new List<uint>(polygon.Count);

			for (int i = 0; i < polygon.Count; i++)
			{
				if (polygon[i].tag != MAP_BORDER_ID && !tags.Contains(polygon[i].tag))
					tags.Add(polygon[i].tag);
			}

			return tags;
		}

		private static void AddMissing(List<VoronoiArea> list, List<VoronoiArea> toAdd)
		{
			foreach (VoronoiArea area in toAdd)
			{
				if (!list.Contains(area))
					list.Add(area);
			}
		}

		private static List<uint> IdsOf(List<VoronoiArea> list)
		{
			List<uint> ids = new List<uint>(list.Count);

			foreach (VoronoiArea area in list)
			{
				ids.Add(area.Node.id);
			}

			return ids;
		}

		private static uint NearestNode(VoronoiNode[] candidates, double x, double y)
		{
			uint nearest = MAP_BORDER_ID;
			double nearestDistance = double.MaxValue;

			for (int i = 0; i < candidates.Length; i++)
			{
				double distanceX = candidates[i].X - x;
				double distanceY = candidates[i].Y - y;
				double distance = distanceX * distanceX + distanceY * distanceY;

				if (distance < nearestDistance || (distance == nearestDistance && candidates[i].id < nearest))
				{
					nearestDistance = distance;
					nearest = candidates[i].id;
				}
			}

			return nearest;
		}

		private void SetPolygon(VoronoiArea area, List<RawPoint> polygon)
		{
			List<VoronoiVertex> newVertices = new List<VoronoiVertex>(polygon.Count);
			List<uint> newTags = new List<uint>(polygon.Count);

			for (int i = 0; i < polygon.Count; i++)
			{
				newVertices.Add(GetOrCreateVertex(polygon[i].x, polygon[i].y));
				newTags.Add(polygon[i].tag);
			}

			List<VoronoiVertex> resolved = new List<VoronoiVertex>(newVertices);
			RemoveRepeatedVertices(newVertices, newTags);
			UpdateAreaGeometry(area, newVertices, newTags);

			foreach (VoronoiVertex vertex in resolved)
			{
				if (vertex.areas.Count == 0)
					vertices.Remove(vertex);
			}
		}

		private void UpdateAreaGeometry(VoronoiArea area, List<VoronoiVertex> newVertices, List<uint> newTags)
		{
			List<VoronoiVertex> oldVertices = area.vertices;

			foreach (VoronoiVertex vertex in newVertices)
			{
				if (!vertex.areas.Contains(area))
					vertex.areas.Add(area);
			}

			area.SetGeometry(newVertices, newTags);

			foreach (VoronoiVertex vertex in oldVertices)
			{
				if (!newVertices.Contains(vertex))
					Release(vertex, area);
			}
		}

		private void Release(VoronoiVertex vertex, VoronoiArea area)
		{
			vertex.areas.Remove(area);

			if (vertex.areas.Count == 0)
				vertices.Remove(vertex);
		}

		private VoronoiVertex GetOrCreateVertex(double x, double y)
		{
			foreach (VoronoiVertex existing in vertices)
			{
				if (System.Math.Abs(existing.x - x) < VERTEX_SNAP && System.Math.Abs(existing.y - y) < VERTEX_SNAP)
					return existing;
			}

			bool isOnMapBorder = x < VERTEX_SNAP || y < VERTEX_SNAP || x > width - VERTEX_SNAP || y > height - VERTEX_SNAP;

			VoronoiVertex vertex = new VoronoiVertex(x, y, isOnMapBorder);
			vertices.Add(vertex);
			return vertex;
		}

		private void Relink(IEnumerable<VoronoiArea> changed)
		{
			List<VoronoiArea> toRelink = new List<VoronoiArea>();

			foreach (VoronoiArea area in changed)
			{
				if (!toRelink.Contains(area))
					toRelink.Add(area);

				foreach (VoronoiArea neighbor in area.neighbors)
				{
					if (areas.ContainsKey(neighbor.Node.id) && !toRelink.Contains(neighbor))
						toRelink.Add(neighbor);
				}
			}

			foreach (VoronoiArea area in toRelink)
			{
				area.neighbors.Clear();

				foreach (uint tag in area.edgeTags)
				{
					if (tag != MAP_BORDER_ID && areas.TryGetValue(tag, out VoronoiArea neighbor) &&
					    !area.neighbors.Contains(neighbor))
						area.neighbors.Add(neighbor);
				}
			}
		}

		// A tile goes to the new node when it is closer to it than to the node that had it.
		private void ClaimTiles(VoronoiNode node)
		{
			(int minX, int minY, int maxX, int maxY) bounds = VoronoiGeometry.TileBounds(areas[node.id].polygon, width, height);

			for (int x = bounds.minX; x <= bounds.maxX; x++)
			{
				for (int y = bounds.minY; y <= bounds.maxY; y++)
				{
					if (!areas.TryGetValue(tileOwners[x, y], out VoronoiArea owner) ||
					    IsCloser(node, owner.Node, x + 0.5, y + 0.5))
						tileOwners[x, y] = node.id;
				}
			}
		}

		// After a collapse the tiles follow the stretched shapes, so the logic is what the drawing shows: every
		// tile of the dead area, or of the ones around it, goes to the area whose polygon now holds its centre.
		private void ReassignTiles(uint deadId, List<VoronoiArea> affected)
		{
			HashSet<uint> affectedIds = new HashSet<uint>(IdsOf(affected));
			VoronoiNode[] affectedNodes = affected.ConvertAll(area => area.Node).ToArray();

			Parallel.For(0, width, x =>
			{
				for (int y = 0; y < height; y++)
				{
					uint owner = tileOwners[x, y];

					if (owner != deadId && !affectedIds.Contains(owner))
						continue;

					uint holder = MAP_BORDER_ID;

					foreach (VoronoiArea area in affected)
					{
						if (VoronoiGeometry.Contains(area.polygon, x + 0.5, y + 0.5))
						{
							holder = area.Node.id;
							break;
						}
					}

					// A centre exactly on a limit can fall in no polygon.
					if (holder != MAP_BORDER_ID)
						tileOwners[x, y] = holder;
					else if (owner == deadId)
						tileOwners[x, y] = NearestNode(affectedNodes, x + 0.5, y + 0.5);
				}
			});

			// The tile a node stands on is always its own, however its area got stretched.
			foreach (VoronoiArea area in affected)
			{
				tileOwners[area.Node.tile.x, area.Node.tile.y] = area.Node.id;
			}
		}

		// The tiles these areas hold go to the closest of their nodes, which is what their exact shapes say.
		private void ShareOutTiles(List<VoronoiArea> list)
		{
			HashSet<uint> ids = new HashSet<uint>(IdsOf(list));
			VoronoiNode[] candidates = list.ConvertAll(area => area.Node).ToArray();

			Parallel.For(0, width, x =>
			{
				for (int y = 0; y < height; y++)
				{
					if (ids.Contains(tileOwners[x, y]))
						tileOwners[x, y] = NearestNode(candidates, x + 0.5, y + 0.5);
				}
			});
		}

		// Same rule as NearestNode: the closest one wins and, on a tie, the lower id.
		private static bool IsCloser(VoronoiNode node, VoronoiNode other, double x, double y)
		{
			double distance = (node.X - x) * (node.X - x) + (node.Y - y) * (node.Y - y);
			double otherDistance = (other.X - x) * (other.X - x) + (other.Y - y) * (other.Y - y);

			return distance < otherDistance || (distance == otherDistance && node.id < other.id);
		}
	}
}
