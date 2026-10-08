using System.Collections.Generic;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Voronoi;
using Parcial.Architecture.GameLogic.Voronoi.Events;
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.GameLogic.Entities.Systems;
using Parcial.View.Scene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Parcial.View.GameLogic.Voronoi
{
    internal sealed class WeightedVoronoiView : VoronoiView
    {
        private EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();

        protected override Color SegmentColor => TeamColorsView.GetColor(0);
        protected override Color VertexColor => TeamColorsView.GetColor(0);
        protected override float LineWidthInCells => 0.16f;

        protected override int DrawingOrder => GameScene.ENTITIES_DRAWING_ORDER + 3;

        private Dictionary<int, WeightedVoronoiDiagram> diagrams;
        private HashSet<int> shownTeams;

        public override void Init()
        {
            base.Init();
            diagrams = new Dictionary<int, WeightedVoronoiDiagram>();
            shownTeams = new HashSet<int>();
            EventBus.SubscribeOnlyMainThread<WeightedVoronoiRecalculatedEvent>(OnWeightedVoronoiRecalculated);
            EventBus.SubscribeOnlyMainThread<QueenKilledEvent>(OnQueenKilled);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            Mouse mouse = Mouse.current;

            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || IsMouseOverUI())
                return;

            if (TryGetTeamOfQueenUnderMouse(out int teamId))
                Toggle(teamId);
        }

        private static bool IsMouseOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private bool TryGetTeamOfQueenUnderMouse(out int teamId)
        {
            Vector3 mousePosition = GameScene.GetMouseWorldPosition();
            Vector2 cellSize = GameScene.GetCellSize;
            float closestDistance = Mathf.Max(cellSize.x, cellSize.y);
            bool found = false;

            teamId = default;

            foreach (QueenAntView queenView in EntityRegistryView.FilterEntities<QueenAntView>())
            {
                float distance = Vector2.Distance(queenView.transform.position, mousePosition);

                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    teamId = queenView.EntityTeamID;
                    found = true;
                }
            }

            return found;
        }

        private void Toggle(int teamId)
        {
            if (!diagrams.ContainsKey(teamId))
                return;

            bool shown = shownTeams.Add(teamId);

            if (!shown)
                shownTeams.Remove(teamId);

            Redraw();
            GameConsole.Log($"Weighted Voronoi of team {teamId} {(shown ? "shown" : "hidden")}");
        }

        private void Redraw()
        {
            BeginDraw();

            foreach (int teamId in shownTeams)
            {
                DrawLimits(diagrams[teamId], TeamColorsView.GetColor(teamId));
            }

            EndDraw();
        }

        private void OnWeightedVoronoiRecalculated(in WeightedVoronoiRecalculatedEvent weightedVoronoiRecalculatedEvent)
        {
            diagrams[weightedVoronoiRecalculatedEvent.teamId] = weightedVoronoiRecalculatedEvent.diagram;

            if (shownTeams.Contains(weightedVoronoiRecalculatedEvent.teamId))
                Redraw();
        }

        private void OnQueenKilled(in QueenKilledEvent queenKilledEvent)
        {
            if (shownTeams.Remove(queenKilledEvent.teamId))
                Redraw();
        }

        public override void Dispose()
        {
            base.Dispose();
            EventBus.UnsubscribeOnlyMainThread<WeightedVoronoiRecalculatedEvent>(OnWeightedVoronoiRecalculated);
            EventBus.UnsubscribeOnlyMainThread<QueenKilledEvent>(OnQueenKilled);
        }
    }
}
