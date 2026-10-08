using System.Collections.Generic;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Voronoi;
using Parcial.Architecture.Math;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Voronoi
{
    internal abstract class VoronoiView : ViewComponent
    {
        private const string LINE_SHADER_NAME = "Sprites/Default";
        private const string LINE_NAME = "Voronoi line";
        private const float VERTEX_WIDTH_IN_CELLS = 0.4f;
        private const float VERTEX_DOT_LENGTH_IN_CELLS = 0.01f;
        private const int CAP_VERTICES = 4;

        protected EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        protected GameScene GameScene => ServiceProvider.Instance.GetService<GameScene>();

        protected abstract Color SegmentColor { get; }
        protected abstract Color VertexColor { get; }
        protected abstract float LineWidthInCells { get; }
        protected abstract int DrawingOrder { get; }

        private List<LineRenderer> lines;
        private Material lineMaterial;
        private int usedLines;

        public override void Init()
        {
            base.Init();
            lines = new List<LineRenderer>();
            lineMaterial = new Material(Shader.Find(LINE_SHADER_NAME));
        }

        protected void Draw(VoronoiDiagram diagram)
        {
            float cellSize = Mathf.Min(GameScene.GetCellSize.x, GameScene.GetCellSize.y);

            BeginDraw();

            foreach (VoronoiArea area in diagram.Areas.Values)
            {
                foreach (VoronoiSegment segment in area.Segments)
                {
                    if (segment.IsMapBorder || area.Node.id > segment.neighborId)
                        continue;

                    DrawLine(MapToWorld(segment.a.x, segment.a.y), MapToWorld(segment.b.x, segment.b.y),
                        cellSize * LineWidthInCells, SegmentColor, 0);
                }
            }

            foreach (VoronoiVertex vertex in diagram.Vertices)
            {
                if (vertex.IsOnMapBorder)
                    continue;

                Vector3 position = MapToWorld(vertex.x, vertex.y);
                DrawLine(position, position + Vector3.right * (cellSize * VERTEX_DOT_LENGTH_IN_CELLS),
                    cellSize * VERTEX_WIDTH_IN_CELLS, VertexColor, CAP_VERTICES);
            }

            EndDraw();
        }

        protected void DrawLimits(WeightedVoronoiDiagram diagram, Color color)
        {
            float cellSize = Mathf.Min(GameScene.GetCellSize.x, GameScene.GetCellSize.y);

            foreach (WeightedVoronoiArea area in diagram.Areas.Values)
            {
                IReadOnlyList<(double x, double y)> polygon = area.Polygon;

                for (int i = 0; i < polygon.Count; i++)
                {
                    uint neighborId = area.NeighborIdOf(i);

                    if (neighborId == VoronoiDiagram.MAP_BORDER_ID || area.Node.id > neighborId)
                        continue;

                    (double x, double y) start = polygon[i];
                    (double x, double y) end = polygon[(i + 1) % polygon.Count];

                    DrawLine(MapToWorld((float)start.x, (float)start.y), MapToWorld((float)end.x, (float)end.y),
                        cellSize * LineWidthInCells, color, 0);
                }
            }
        }

        protected void BeginDraw()
        {
            usedLines = 0;
        }

        protected void EndDraw()
        {
            for (int i = usedLines; i < lines.Count; i++)
            {
                lines[i].gameObject.SetActive(false);
            }
        }

        private void DrawLine(Vector3 from, Vector3 to, float lineWidth, Color color, int capVertices)
        {
            if (usedLines == lines.Count)
            {
                GameObject lineObject = new GameObject(LINE_NAME);
                lineObject.transform.parent = transform;

                LineRenderer newLine = lineObject.AddComponent<LineRenderer>();
                newLine.sharedMaterial = lineMaterial;
                newLine.useWorldSpace = true;
                newLine.positionCount = 2;
                newLine.sortingOrder = DrawingOrder;
                lines.Add(newLine);
            }

            LineRenderer line = lines[usedLines];
            usedLines++;

            line.gameObject.SetActive(true);
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = capVertices;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }

        private Vector3 MapToWorld(float x, float y)
        {
            Vector3 origin = GameScene.PointToWorld(new Point(0, 0));
            Vector3 stepX = GameScene.PointToWorld(new Point(1, 0)) - origin;
            Vector3 stepY = GameScene.PointToWorld(new Point(0, 1)) - origin;

            return origin + stepX * (x - 0.5f) + stepY * (y - 0.5f);
        }

        public override void Dispose()
        {
            base.Dispose();
            Destroy(lineMaterial);
        }
    }
}
