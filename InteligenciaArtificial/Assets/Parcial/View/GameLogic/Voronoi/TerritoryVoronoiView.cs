using Parcial.Architecture.GameLogic.Voronoi.Events;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Voronoi
{
    internal sealed class TerritoryVoronoiView : VoronoiView
    {
        protected override Color SegmentColor => Color.red;
        protected override Color VertexColor => Color.magenta;
        protected override float LineWidthInCells => 0.2f;

        protected override int DrawingOrder => GameScene.ENTITIES_DRAWING_ORDER + 1;

        public override void Init()
        {
            base.Init();
            EventBus.SubscribeOnlyMainThread<TerritoryVoronoiRecalculatedEvent>(OnTerritoryVoronoiRecalculated);
        }

        private void OnTerritoryVoronoiRecalculated(in TerritoryVoronoiRecalculatedEvent territoryVoronoiRecalculatedEvent)
        {
            Draw(territoryVoronoiRecalculatedEvent.diagram);
        }

        public override void Dispose()
        {
            base.Dispose();
            EventBus.UnsubscribeOnlyMainThread<TerritoryVoronoiRecalculatedEvent>(OnTerritoryVoronoiRecalculated);
        }
    }
}
